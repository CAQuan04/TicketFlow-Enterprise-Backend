using MediatR;

namespace TicketBooking.Application.Features.Tickets.Queries.GetMyTickets
{
    // Query trả về danh sách vé của người dùng hiện tại.
    public record GetMyTicketsQuery : IRequest<List<MyTicketDto>>;
}