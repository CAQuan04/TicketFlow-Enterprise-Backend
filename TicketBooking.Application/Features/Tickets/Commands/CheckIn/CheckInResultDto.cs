namespace TicketBooking.Application.Features.Tickets.Commands.CheckIn
{
    public record CheckInResultDto(
        string EventName,
        string TicketTypeName, // Hạng vé (VIP/Thường).
        string CustomerName,
        string TicketCode,
        DateTime CheckInTime,
        string Message
    );
}