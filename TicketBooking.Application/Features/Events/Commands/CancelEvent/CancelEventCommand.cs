using MediatR;
namespace TicketBooking.Application.Features.Events.Commands.CancelEvent
{
    public record CancelEventCommand(Guid EventId) : IRequest;
}