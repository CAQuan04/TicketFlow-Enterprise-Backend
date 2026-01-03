namespace TicketBooking.Application.Features.Orders.Commands.CreateOrder
{
    public record OrderItemDto(
        Guid TicketTypeId,
        int Quantity
    );
}