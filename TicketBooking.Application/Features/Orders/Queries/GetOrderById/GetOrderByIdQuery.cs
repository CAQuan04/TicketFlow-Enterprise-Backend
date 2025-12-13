using MediatR;

namespace TicketBooking.Application.Features.Orders.Queries.GetOrderById
{
    public record GetOrderByIdQuery(Guid OrderId) : IRequest<OrderDto>;
}