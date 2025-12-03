using MediatR; // Import MediatR.
using TicketBooking.Domain.Enums; // Import Enums.

namespace TicketBooking.Application.Features.Auth.Commands.SocialLogin
{
    // Command to initiate social login. Returns JWT Token string.
    public record SocialLoginCommand(
        string Token, // The ID Token from the Frontend (Google).
        LoginProvider Provider // Which provider is being used (Google/Facebook).
    ) : IRequest<string>;
}