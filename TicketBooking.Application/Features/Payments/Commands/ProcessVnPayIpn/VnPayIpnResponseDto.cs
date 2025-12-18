namespace TicketBooking.Application.Features.Payments.Commands.ProcessVnPayIpn
{
    // Standard JSON format required by VNPay IPN.
    public record VnPayIpnResponseDto(string RspCode, string Message);
}