using MediatR; // Dùng cho IRequestHandler.
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore; // Dùng cho các hàm Async của EF Core.
using Microsoft.Extensions.Caching.Distributed; // Dùng cho Redis Cache.
using System.Text.Json; // Dùng để serialize object lưu vào Redis.
using TicketBooking.Application.Common.Exceptions; // Dùng Custom Exceptions.
using TicketBooking.Application.Common.Interfaces; // Dùng DbContext.
using TicketBooking.Application.Common.Interfaces.RealTime;
using TicketBooking.Domain.Entities; // Dùng Entity Order, Ticket.
using TicketBooking.Domain.Enums; // Dùng Enum OrderStatus.

namespace TicketBooking.Application.Features.Orders.Commands.CreateOrder
{
    // Handler xử lý logic đặt vé với Transaction và Concurrency Check.
    public class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, Guid>
    {
        private readonly IApplicationDbContext _context; // Truy cập Database.
        private readonly ICurrentUserService _currentUserService; // Lấy ID người đang login.
        private readonly IDistributedCache _distributedCache; // Truy cập Redis.
        private readonly INotificationService _notificationService;
        // Constructor Injection.
        public CreateOrderCommandHandler(
            IApplicationDbContext context,
            ICurrentUserService currentUserService,
            IDistributedCache distributedCache,
            INotificationService notificationService)
        {
            _context = context;
            _currentUserService = currentUserService;
            _distributedCache = distributedCache;
            _notificationService = notificationService;
        }

        public async Task<Guid> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
        {
            // 1. TRANSACTION SCOPE START (ACID: Atomicity)
            // Bắt đầu một Transaction database. Tất cả hành động sau dòng này phải thành công cùng nhau, hoặc thất bại cùng nhau.
            // Nếu có lỗi, mọi thay đổi DB sẽ được rollback (hoàn tác).
            using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                // 2. INVENTORY CHECK (ACID: Consistency)
                // Lấy thông tin loại vé từ Database.
                var ticketType = await _context.TicketTypes
                    .FirstOrDefaultAsync(t => t.Id == request.TicketTypeId, cancellationToken); // Sửa lại query ID cho đúng Entity.

                // Kiểm tra vé có tồn tại không.
                if (ticketType == null)
                {
                    throw new NotFoundException(nameof(TicketType), request.TicketTypeId);
                }

                // Kiểm tra số lượng vé còn lại có đủ cho yêu cầu không.
                if (ticketType.AvailableQuantity < request.Quantity)
                {
                    // Nếu không đủ, ném lỗi Validation để báo Client.
                    throw new ValidationException();
                    // (Sếp lưu ý: Nên truyền Dictionary lỗi chi tiết vào đây như bài trước đã làm).
                }

                // 3. DEDUCT INVENTORY (ACID: Isolation start)
                // Trừ số lượng vé khả dụng.
                // QUAN TRỌNG: Lúc này EF Core sẽ đánh dấu dòng này bị thay đổi (Dirty).
                // Trường RowVersion cũng sẽ được tham gia vào câu lệnh UPDATE SQL sau này.
                ticketType.AvailableQuantity -= request.Quantity;

                // 4. CREATE ORDER
                // Lấy UserID hiện tại.
                var userId = Guid.Parse(_currentUserService.UserId!);

                // Tạo đối tượng Order mới.
                var order = new Order
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    OrderCode = Guid.NewGuid().ToString().Substring(0, 8).ToUpper(), // Mã đơn hàng ngắn gọn.
                    TotalAmount = ticketType.Price * request.Quantity, // Tính tổng tiền.
                    Status = OrderStatus.Pending, // Trạng thái chờ thanh toán.
                    OrderDate = DateTime.UtcNow,
                    // Logic nghiệp vụ: Giữ vé trong 10 phút. Nếu không thanh toán sẽ hủy.
                    CreatedDate = DateTime.UtcNow
                };

                // Tạo các vé con (Tickets) gắn với Order này.
                for (int i = 0; i < request.Quantity; i++)
                {
                    var ticket = new Ticket
                    {
                        Id = Guid.NewGuid(),
                        OrderId = order.Id,
                        TicketTypeId = ticketType.Id,
                        TicketCode = Guid.NewGuid().ToString(), // Mã vé riêng biệt.
                        CreatedDate = DateTime.UtcNow
                    };
                    // Thêm vào list vé của Order (EF Core tự hiểu quan hệ).
                    order.Tickets.Add(ticket);
                }

                // Thêm Order vào DbContext (vẫn chỉ là Memory, chưa xuống DB).
                _context.Orders.Add(order);

                // 5. CONCURRENCY HANDLING (CRITICAL STEP)
                // Gọi SaveChangesAsync để đẩy lệnh UPDATE xuống SQL Server.
                // Tại đây, SQL Server sẽ chạy câu lệnh dạng:
                // UPDATE TicketTypes SET AvailableQuantity = ..., RowVersion = NewVal 
                // WHERE Id = ... AND RowVersion = OldVal
                await _context.SaveChangesAsync(cancellationToken);

                // 6. COMMIT TRANSACTION (ACID: Durability)
                // Nếu chạy đến dòng này nghĩa là không có xung đột RowVersion.
                // Chúng ta chốt giao dịch.
                await transaction.CommitAsync(cancellationToken);

                // --- 🔥 REAL-TIME FOMO BROADCAST (DÙNG INTERFACE) ---
                // Gửi thông báo cập nhật kho vé qua Interface
                // Bên trong implementation (Infrastructure) sẽ xử lý logic SignalR
                await _notificationService.SendToGroupAsync(
                    ticketType.EventId.ToString(),
                    "UpdateInventory",
                    ticketType.AvailableQuantity
                );

                // 7. REDIS CACHING (PERFORMANCE)
                // Lưu trạng thái đơn hàng vào Redis để truy xuất nhanh (ví dụ cho trang Payment check status).
                // Key: "Order:{Id}", Value: "Pending", Expire: 10 mins.
                var cacheKey = $"Order:{order.Id}";
                var cacheValue = JsonSerializer.Serialize(new { Status = "Pending", TotalAmount = order.TotalAmount });

                await _distributedCache.SetStringAsync(cacheKey, cacheValue, new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10) // Cache hết hạn sau 10p (khớp logic giữ vé).
                }, cancellationToken);

                // Trả về OrderId cho Client để họ chuyển sang trang thanh toán.
                return order.Id;
            }
            catch (DbUpdateConcurrencyException)
            {
                // Xảy ra khi: 2 người cùng đọc AvailableQuantity = 1.
                // Người A save trước -> RowVersion thay đổi.
                // Người B save sau -> RowVersion không khớp -> SQL trả về 0 row affected -> EF Core ném Exception này.

                // Rollback transaction (dù using block tự làm, nhưng gọi explicit cho rõ ràng).
                await transaction.RollbackAsync(cancellationToken);

                // Ném lỗi 409 Conflict để Client biết mà thử lại hoặc báo "Hết vé".
                throw new ConflictException("Ticket sold out just now or modified by another user. Please retry.");
            }
            catch (Exception)
            {
                // Bắt các lỗi khác (DB connection, Logic...) và Rollback an toàn.
                await transaction.RollbackAsync(cancellationToken);
                throw; // Ném tiếp lỗi ra ngoài cho GlobalHandler xử lý.
            }
        }
    }
}