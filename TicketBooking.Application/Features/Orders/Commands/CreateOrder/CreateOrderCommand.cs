using MediatR;

namespace TicketBooking.Application.Features.Orders.Commands.CreateOrder
{  
    public record CreateOrderCommand(
        List<OrderItemDto> Items
    ) : IRequest<Guid>;
}