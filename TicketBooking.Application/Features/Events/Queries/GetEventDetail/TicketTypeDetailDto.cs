namespace TicketBooking.Application.Features.Events.Queries.GetEventDetail
{
    public record TicketTypeDetailDto(
        Guid Id,
        string Name,
        decimal Price,
        decimal? OriginalPrice,
        string? Description,
        int AvailableQuantity // Khách cần biết còn vé hay không.
    );
}