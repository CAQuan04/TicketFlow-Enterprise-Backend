using MediatR;

namespace TicketBooking.Application.Features.Events.Commands.ApproveEvent
{
    // Define command to approve an event.
    // We pass the EventId via the command.
    public record ApproveEventCommand(Guid EventId) : IRequest;
}