using MediatR; // Import thư viện MediatR.

namespace TicketBooking.Application.Features.Organizers.Queries.GetEventStats
{
    // Query yêu cầu lấy thống kê cho một Event cụ thể.
    // Input: EventId (Guid). Output: OrganizerEventStatsDto.
    public record GetEventStatsQuery(Guid EventId) : IRequest<OrganizerEventStatsDto>;
}