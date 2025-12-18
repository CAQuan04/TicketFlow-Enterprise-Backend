using MediatR;
using Microsoft.AspNetCore.Http; // Needed for IQueryCollection

namespace TicketBooking.Application.Features.Payments.Commands.ProcessVnPayIpn
{
    // Command to process IPN. We pass the entire Query Collection to capture all dynamic params.
    public record ProcessVnPayIpnCommand(IQueryCollection QueryData) : IRequest<VnPayIpnResponseDto>;
}