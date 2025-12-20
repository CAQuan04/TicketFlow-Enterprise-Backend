using MediatR;

namespace TicketBooking.Application.Features.Tickets.Commands.CheckIn
{
    // Input là mã vé (String) đọc được từ QR Code Scanner.
    public record CheckInTicketCommand(string TicketCode) : IRequest<CheckInResultDto>;
}