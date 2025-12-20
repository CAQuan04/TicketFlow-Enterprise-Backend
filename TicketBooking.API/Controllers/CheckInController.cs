using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TicketBooking.API.Controllers;
using TicketBooking.Application.Features.Tickets.Commands.CheckIn;
using TicketBooking.Domain.Constants;

namespace TicketBooking.API.Controllers
{
    // API dành riêng cho nghiệp vụ Soát vé.
    public class CheckInController : ApiControllerBase
    {
        // Endpoint: POST api/CheckIn
        // Bảo mật: Chỉ User có Role "TicketInspector" mới được gọi.
        // Hacker dù có Token Customer cũng không thể tự Check-in.
        [Authorize(Roles = Roles.TicketInspector)]
        [HttpPost]
        public async Task<IActionResult> CheckIn([FromBody] CheckInTicketCommand command)
        {
            var result = await Mediator.Send(command);

            // Trả về 200 OK kèm thông tin để App hiện màn hình Xanh lá cây (Success).
            return Ok(result);
        }
    }
}