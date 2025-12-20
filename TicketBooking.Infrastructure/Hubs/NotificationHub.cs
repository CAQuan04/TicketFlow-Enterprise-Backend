using Microsoft.AspNetCore.Authorization; // Dùng để bảo vệ Hub.
using Microsoft.AspNetCore.SignalR; // Dùng thư viện SignalR.
using TicketBooking.Application.Common.Interfaces.RealTime; // Import Interface Client.

namespace TicketBooking.Infrastructure.Hubs
{
    // Yêu cầu bắt buộc phải có Token mới được kết nối vào Hub này.
    [Authorize]
    public class NotificationHub : Hub<INotificationClient>
    {
        // Hàm này chạy mỗi khi có một Client kết nối thành công.
        public override async Task OnConnectedAsync()
        {
            // Lấy UserId từ Claims (Do [Authorize] đã validate token và gán vào Context).
            var userId = Context.UserIdentifier;

            // Trong mô hình Enterprise đơn giản, SignalR tự động quản lý mapping UserId -> ConnectionId
            // thông qua IUserIdProvider mặc định (dựa trên ClaimTypes.NameIdentifier).
            // Do đó, ta không cần Dictionary thủ công trừ khi muốn track trạng thái Online/Offline chi tiết.

            // Gửi lời chào cá nhân hóa (Optional).
            await Clients.Caller.ReceiveNotification($"Connected successfully! Welcome user {userId}");

            await base.OnConnectedAsync();
        }

        // Client (Frontend) sẽ gọi hàm này khi họ mở trang chi tiết sự kiện.
        // Ví dụ: connection.invoke("JoinEventGroup", "event-guid-id");
        public async Task JoinEventGroup(string eventId)
        {
            // Thêm Connection hiện tại vào nhóm có tên là EventId.
            // SignalR tự quản lý danh sách này trong bộ nhớ.
            await Groups.AddToGroupAsync(Context.ConnectionId, eventId);
        }

        // Client gọi hàm này khi họ rời trang sự kiện.
        public async Task LeaveEventGroup(string eventId)
        {
            // Xóa khỏi nhóm để không nhận thông báo rác nữa.
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, eventId);
        }
    }
}