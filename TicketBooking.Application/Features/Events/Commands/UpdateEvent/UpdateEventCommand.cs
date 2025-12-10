using MediatR;

namespace TicketBooking.Application.Features.Events.Commands.UpdateEvent
{
    public record UpdateEventCommand(
        Guid EventId,
        string Name,
        string Description,
        string? CoverImageUrl,
        DateTime StartDateTime,
        DateTime EndDateTime,
        List<TicketTypeUpdateDto> TicketTypes // Danh sách vé.
    ) : IRequest;
}