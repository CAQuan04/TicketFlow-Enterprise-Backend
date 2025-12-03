using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TicketBooking.Application.Features.Users.Commands.CreateUser
{
    // Defined a record for the command which is immutable and concise.
    // Added 'Password' property to receive the raw password from the API request.
    public record CreateUserCommand(string FullName, string Email, string Password) : IRequest<string>;


}
