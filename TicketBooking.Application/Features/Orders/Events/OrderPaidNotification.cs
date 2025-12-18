using MediatR;

namespace TicketBooking.Application.Features.Orders.Events
{
    // Sự kiện nội bộ: "Đơn hàng đã được thanh toán xong".
    // Các Handler khác sẽ lắng nghe sự kiện này để làm việc riêng (Gửi mail, Thống kê...).
    public record OrderPaidNotification(Guid OrderId) : INotification;
}