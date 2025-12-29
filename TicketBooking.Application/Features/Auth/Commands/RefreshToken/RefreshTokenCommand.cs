using MediatR;
using TicketBooking.Application.Features.Auth.DTOs;

namespace TicketBooking.Application.Features.Auth.Commands.RefreshToken
{
    public record RefreshTokenCommand(string AccessToken, string RefreshToken) : IRequest<AuthResponse>;
}