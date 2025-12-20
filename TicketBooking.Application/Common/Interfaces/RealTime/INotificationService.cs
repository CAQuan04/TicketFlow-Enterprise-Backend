namespace TicketBooking.Application.Common.Interfaces.RealTime
{
    // Interface này giúp Application gửi tin nhắn mà không cần biết đến SignalR Hub.
    public interface INotificationService
    {
        // Gửi thông báo cho 1 user cụ thể
        Task SendToUserAsync(string userId, string message);

        // Gửi thông báo cho cả nhóm (ví dụ nhóm đang xem Event)
        Task SendToGroupAsync(string groupName, string messageType, object data);
    }
}