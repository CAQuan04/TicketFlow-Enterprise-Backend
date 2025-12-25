using TicketBooking.Application.Features.Events.Queries.GetEventsList; // Reuse EventListDto

namespace TicketBooking.Application.Common.Interfaces
{
    public interface ISearchService
    {
        Task<List<EventListDto>> SearchAsync(string keyword, CancellationToken cancellationToken);
    }
}