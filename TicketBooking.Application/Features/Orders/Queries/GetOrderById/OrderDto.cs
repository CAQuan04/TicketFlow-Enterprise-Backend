namespace TicketBooking.Application.Features.Orders.Queries.GetOrderById
{
    public record OrderDto(
        Guid Id,
        string OrderCode,
        decimal TotalAmount,
        string Status,
        DateTime CreatedDate
    );
}