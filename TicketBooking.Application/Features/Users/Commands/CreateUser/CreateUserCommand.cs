using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TicketBooking.Application.Features.Users.Commands.CreateUser
{
    // Lệnh này yêu cầu trả về Guid (Id của User vừa tạo)
    public record CreateUserCommand(string FullName, string Email) : IRequest<Guid>;


}
