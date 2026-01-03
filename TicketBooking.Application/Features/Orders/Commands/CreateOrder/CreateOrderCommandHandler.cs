using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;
using TicketBooking.Application.Common.Exceptions;
using TicketBooking.Application.Common.Interfaces;
using TicketBooking.Application.Common.Interfaces.RealTime;
using TicketBooking.Domain.Entities;
using TicketBooking.Domain.Enums;

namespace TicketBooking.Application.Features.Orders.Commands.CreateOrder
{
    public class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, Guid>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUserService;
        private readonly IDistributedCache _distributedCache;
        private readonly INotificationService _notificationService;

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
            var userId = Guid.Parse(_currentUserService.UserId!);
            var now = DateTime.UtcNow;

            // 1. START TRANSACTION (ACID)
            using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                // 2. PREPARE DATA
                // Lấy tất cả ID vé khách muốn mua
                var requestTicketTypeIds = request.Items.Select(x => x.TicketTypeId).ToList();

                // Load tất cả TicketTypes từ DB trong 1 Query (Kèm Event)
                // Dùng Tracking để update inventory
                var dbTicketTypes = await _context.TicketTypes
                    .Include(t => t.Event)
                    .Where(t => requestTicketTypeIds.Contains(t.Id))
                    .ToListAsync(cancellationToken);

                // Check xem có ID nào gửi lên mà không tồn tại trong DB không
                if (dbTicketTypes.Count != requestTicketTypeIds.Distinct().Count())
                {
                    throw new ValidationException("Một số loại vé không tồn tại hoặc không hợp lệ.");
                }

                // 3. VALIDATE EVENT CONSISTENCY (Quy tắc nghiệp vụ: 1 Đơn chỉ mua cho 1 Sự kiện)
                // Để đơn giản hóa logic thanh toán và check limit, ta bắt buộc các vé phải cùng 1 Event.
                var firstEventId = dbTicketTypes.First().EventId;
                if (dbTicketTypes.Any(t => t.EventId != firstEventId))
                {
                    throw new ValidationException("Trong một đơn hàng chỉ được mua vé của cùng một sự kiện.");
                }

                var eventInfo = dbTicketTypes.First().Event;

                // 4. CHECK EVENT RULES (Time & Status)
                if (now < eventInfo.TicketSaleStartTime)
                    throw new ValidationException("Sự kiện chưa mở bán.");

                if (eventInfo.TicketSaleEndTime.HasValue && now > eventInfo.TicketSaleEndTime.Value)
                    throw new ValidationException("Đã hết thời gian bán vé.");

                // 5. CHECK PURCHASE LIMIT (Giới hạn mua của User với Sự kiện này)
                int totalRequestedQty = request.Items.Sum(x => x.Quantity);

                if (eventInfo.MaxTicketsPerUser > 0)
                {
                    // Đếm số vé đã mua trong quá khứ
                    var boughtCount = await _context.Tickets
                        .Where(t => t.Order.UserId == userId
                                 && t.TicketType.EventId == firstEventId
                                 && t.Order.Status != OrderStatus.Cancelled)
                        .CountAsync(cancellationToken);

                    if (boughtCount + totalRequestedQty > eventInfo.MaxTicketsPerUser)
                    {
                        throw new ValidationException(
                            $"Bạn chỉ được mua tối đa {eventInfo.MaxTicketsPerUser} vé cho sự kiện này. " +
                            $"Bạn đã mua {boughtCount} vé, đang yêu cầu thêm {totalRequestedQty} vé.");
                    }
                }

                // 6. INIT ORDER
                var order = new Order
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    OrderCode = Guid.NewGuid().ToString().Substring(0, 8).ToUpper(),
                    Status = OrderStatus.Pending,
                    OrderDate = now,
                    CreatedDate = now,
                    Tickets = new List<Ticket>()
                };

                decimal totalAmount = 0;

                // 7. PROCESS EACH ITEM (Inventory Deduct & Ticket Creation)
                foreach (var item in request.Items)
                {
                    var ticketType = dbTicketTypes.First(t => t.Id == item.TicketTypeId);

                    // Check Kho
                    if (ticketType.AvailableQuantity < item.Quantity)
                    {
                        throw new ValidationException($"Loại vé '{ticketType.Name}' không đủ số lượng. Còn lại: {ticketType.AvailableQuantity}");
                    }

                    // Trừ Kho
                    ticketType.AvailableQuantity -= item.Quantity;

                    // Tính tiền
                    totalAmount += ticketType.Price * item.Quantity;

                    // Tạo vé con
                    for (int i = 0; i < item.Quantity; i++)
                    {
                        order.Tickets.Add(new Ticket
                        {
                            Id = Guid.NewGuid(),
                            OrderId = order.Id,
                            TicketTypeId = ticketType.Id,
                            TicketCode = Guid.NewGuid().ToString(), // Tạm thời
                            CreatedDate = now
                        });
                    }
                }

                order.TotalAmount = totalAmount;
                _context.Orders.Add(order);

                // 8. SAVE & COMMIT
                await _context.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                // 9. BROADCAST INVENTORY UPDATE (FOMO)
                // Gửi thông báo cho từng loại vé bị thay đổi
                foreach (var dbTicketType in dbTicketTypes)
                {
                    await _notificationService.SendToGroupAsync(
                        dbTicketType.EventId.ToString(),
                        "UpdateInventory",
                        dbTicketType.AvailableQuantity
                    );
                }

                // 10. CACHE
                var cacheKey = $"Order:{order.Id}";
                var cacheValue = JsonSerializer.Serialize(new { Status = "Pending", TotalAmount = order.TotalAmount });
                await _distributedCache.SetStringAsync(cacheKey, cacheValue, new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10)
                }, cancellationToken);

                return order.Id;
            }
            catch (DbUpdateConcurrencyException)
            {
                await transaction.RollbackAsync(cancellationToken);
                throw new ConflictException("Vé vừa bị người khác mua mất. Vui lòng thử lại.");
            }
            catch
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        }
    }
}