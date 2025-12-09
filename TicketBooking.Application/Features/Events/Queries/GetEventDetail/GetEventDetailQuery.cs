using MediatR;

namespace TicketBooking.Application.Features.Events.Queries.GetEventDetail
{
    // Query yêu cầu lấy chi tiết sự kiện theo ID.
    // Trả về đối tượng EventDetailDto.
    public record GetEventDetailQuery(Guid EventId) : IRequest<EventDetailDto>;
}