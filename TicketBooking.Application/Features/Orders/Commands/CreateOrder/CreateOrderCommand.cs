using MediatR;

namespace TicketBooking.Application.Features.Orders.Commands.CreateOrder
{
    // Lệnh tạo đơn hàng. Trả về OrderId (Guid).
    public record CreateOrderCommand(
        Guid TicketTypeId, // Khách mua loại vé nào.
        int Quantity       // Mua bao nhiêu vé.
    ) : IRequest<Guid>;
}