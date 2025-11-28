using Microsoft.AspNetCore.Mvc;
using TicketBooking.Application.Features.Users.Commands.CreateUser;

namespace TicketBooking.API.Controllers
{
    public class UsersController : ApiControllerBase
    {
        [HttpPost]
        public async Task<IActionResult> Create(CreateUserCommand command)
        {
            // Gửi lệnh CreateUserCommand đến Handler tương ứng trong Application Layer
            var userId = await Mediator.Send(command);

            // Trả về kết quả 200 OK kèm theo UserId vừa tạo
            return Ok(userId);
        }
    }
}