using MediatR; // Import MediatR.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TicketBooking.Application.Features.Auth.DTOs;

namespace TicketBooking.Application.Features.Auth.Queries.Login
{
    // Đổi return type từ string sang AuthResponse
    public record LoginQuery(string Email, string Password) : IRequest<AuthResponse>;
}