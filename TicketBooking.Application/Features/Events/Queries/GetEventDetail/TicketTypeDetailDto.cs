namespace TicketBooking.Application.Features.Events.Queries.GetEventDetail
{
    public record TicketTypeDetailDto(
        Guid Id,
        string Name,
        decimal Price,
        int AvailableQuantity // Khách cần biết còn vé hay không.
    );
}