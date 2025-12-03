using MediatR;

namespace TicketBooking.Application.Features.Auth.Commands.VerifyEmail
{
    // Command to verify email using OTP.
    public record VerifyEmailCommand(string Email, string Otp) : IRequest<string>;
}