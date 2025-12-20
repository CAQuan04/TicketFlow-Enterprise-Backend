using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore; // Dùng để truy vấn DB.
using Microsoft.Extensions.DependencyInjection; // Dùng để tạo Scope.
using Microsoft.Extensions.Hosting; // Dùng cho BackgroundService.
using Microsoft.Extensions.Logging; // Dùng để ghi Log.
using TicketBooking.Application.Common.Interfaces.RealTime;
using TicketBooking.Domain.Enums; // Dùng OrderStatus.
using TicketBooking.Infrastructure.Data;
using TicketBooking.Infrastructure.Hubs; // Dùng ApplicationDbContext.

namespace TicketBooking.Infrastructure.BackgroundJobs
{
    // Worker chạy ngầm, tự động quét và thu hồi vé từ các đơn hàng hết hạn.
    public class ExpiredOrderCleanupWorker : BackgroundService
    {
        // Factory dùng để tạo ra một Scope mới (bắt buộc vì Worker là Singleton).
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly ILogger<ExpiredOrderCleanupWorker> _logger;
        // Inject HubContext vào Singleton Service vẫn hoạt động tốt (Thread-safe).
        private readonly IHubContext<NotificationHub, INotificationClient> _hubContext;
        public ExpiredOrderCleanupWorker(
            IServiceScopeFactory serviceScopeFactory,
            ILogger<ExpiredOrderCleanupWorker> logger,
            IHubContext<NotificationHub, INotificationClient> hubContext)
        {
            _serviceScopeFactory = serviceScopeFactory;
            _logger = logger;
            _hubContext = hubContext;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Expired Order Cleanup Worker started.");

            // Sử dụng PeriodicTimer: Cách hiện đại và chính xác nhất để chạy job định kỳ trong .NET 6+.
            // Nó tốt hơn Task.Delay vì nó không bị trôi thời gian (Time Drift) sau nhiều lần chạy.
            using var timer = new PeriodicTimer(TimeSpan.FromMinutes(1));

            // Vòng lặp vô tận, chạy mỗi khi timer "tick" (1 phút/lần) cho đến khi app tắt.
            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                try
                {
                    await ProcessExpiredOrdersAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    // Catch All: Đảm bảo nếu có lỗi xảy ra, Worker KHÔNG ĐƯỢC PHÉP CHẾT (Crash).
                    // Nó phải sống để tiếp tục chạy vào phút tiếp theo.
                    _logger.LogError(ex, "Error occurred while cleaning up expired orders.");
                }
            }
        }

        private async Task ProcessExpiredOrdersAsync(CancellationToken stoppingToken)
        {
            // 1. CREATE SCOPE
            // DbContext là Scoped Service (sống theo Request). Worker là Singleton (sống mãi mãi).
            // Ta phải tạo thủ công một Scope để lấy được DbContext.
            using var scope = _serviceScopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            // 2. DEFINE THRESHOLD (NGƯỠNG THỜI GIAN)
            // Lấy các đơn hàng tạo quá 10 phút trước.
            var timeThreshold = DateTime.UtcNow.AddMinutes(-10);

            // 3. FETCH DATA (EAGER LOADING)
            // Lấy đơn Pending + quá hạn.
            // Include Tickets và TicketType để lát nữa cộng lại kho (Restore Inventory).
            var expiredOrders = await context.Orders
                .Include(o => o.Tickets)
                    .ThenInclude(t => t.TicketType) // Load sâu vào TicketType để update AvailableQuantity.
                .Where(o => o.Status == OrderStatus.Pending && o.CreatedDate < timeThreshold)
                .ToListAsync(stoppingToken);

            // Nếu không có đơn nào hết hạn thì thoát sớm cho nhẹ.
            if (!expiredOrders.Any()) return;

            _logger.LogInformation($"Found {expiredOrders.Count} expired orders to cancel.");

            // 4. PROCESS LOGIC (BUSINESS RULES)
            foreach (var order in expiredOrders)
            {
                // A. Đổi trạng thái sang Cancelled.
                // Lưu ý: Sếp có thể thêm trạng thái "Expired" vào Enum nếu muốn phân biệt rõ hơn.
                // Ở đây tôi dùng Cancelled theo yêu cầu.
                order.Status = OrderStatus.Cancelled;

                // B. Hoàn trả kho vé (Inventory Recovery).
                // Vì 1 Order có thể chứa nhiều vé, ta đếm số lượng vé trong Order đó.
                // Giả định: Trong 1 Order các vé thường cùng loại (theo logic CreateOrder bài trước).
                // Tuy nhiên code này hỗ trợ cả trường hợp Order có nhiều loại vé khác nhau (Robustness).

                // --- 1. NOTIFY USER (PERSONAL) ---
                // Báo cho người mua biết họ đã mất vé.
                await _hubContext.Clients.User(order.UserId.ToString())
                    .ReceiveNotification($"Đơn hàng #{order.OrderCode} đã bị hủy do hết hạn thanh toán.");

                // Gom nhóm các vé theo loại (TicketType) để cộng dồn số lượng trả lại.
                var ticketsByTypes = order.Tickets.GroupBy(t => t.TicketType);

                foreach (var group in ticketsByTypes)
                {
                    var ticketType = group.Key;
                    var quantityToRestore = group.Count();

                    // Cộng ngược lại vào kho.
                    // EF Core đang theo dõi ticketType này (nhờ Include ở trên), nên nó sẽ tự generate UPDATE SQL.
                    ticketType.AvailableQuantity += quantityToRestore;

                    // --- 2. NOTIFY GROUP (BROADCAST INVENTORY) ---
                    // Báo cho tất cả những ai đang xem Event này là số lượng vé đã thay đổi.
                    // Group Name là EventId (string).
                    await _hubContext.Clients.Group(ticketType.EventId.ToString())
                        .UpdateInventory(ticketType.EventId, ticketType.AvailableQuantity);

                    _logger.LogInformation($"Restored {quantityToRestore} tickets for Type ID: {ticketType.Id} from Order {order.OrderCode}");
                }
            }

            // 5. SAVE CHANGES (TRANSACTION)
            // Lưu tất cả thay đổi (Update Order Status + Update TicketType Quantity) trong 1 Transaction.
            await context.SaveChangesAsync(stoppingToken);
        }
    }
}