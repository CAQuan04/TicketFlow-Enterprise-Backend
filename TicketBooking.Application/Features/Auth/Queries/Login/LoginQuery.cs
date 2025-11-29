using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MediatR; // Import MediatR.

namespace TicketBooking.Application.Features.Auth.Queries.Login
{
    // This query represents a login request.
    // It returns a 'string', which will be the JWT Token.
    public record LoginQuery(string Email, string Password) : IRequest<string>;
}