using MediatR;

namespace TicketBooking.Application.Features.Orders.Commands.PayOrder
{
    // Lệnh: Thanh toán đơn hàng bằng số dư ví.
    public record PayOrderCommand(Guid OrderId) : IRequest<bool>;
}