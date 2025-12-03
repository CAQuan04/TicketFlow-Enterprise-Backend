using MediatR; // Import MediatR.

namespace TicketBooking.Application.Features.Events.Commands.CreateEvent
{
    // Define the command to create an event.
    public record CreateEventCommand(
        Guid VenueId, // The ID of the venue where the event takes place.
        string Name, // Name of the event.
        string Description, // Detailed description.
        DateTime EventDate // When the event happens.
    ) : IRequest<Guid>;
}