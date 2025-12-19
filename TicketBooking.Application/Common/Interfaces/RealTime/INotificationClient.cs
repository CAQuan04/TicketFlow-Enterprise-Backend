namespace TicketBooking.Application.Common.Interfaces.RealTime
{
    // Interface định nghĩa các hàm mà Server có thể gọi xuống Client (Frontend/Mobile).
    public interface INotificationClient
    {
        // Gửi thông báo dạng text đơn thuần (Toast notification).
        Task ReceiveNotification(string message);

        // Cập nhật số lượng vé tồn kho theo thời gian thực (cho trang chi tiết sự kiện).
        Task UpdateInventory(Guid eventId, int remainingSeats);
    }
}