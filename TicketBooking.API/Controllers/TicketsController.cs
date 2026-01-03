using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TicketBooking.API.Controllers; // BaseController
using TicketBooking.Application.Features.Tickets.Queries.GetMyTickets;
using TicketBooking.Domain.Constants;

namespace TicketBooking.API.Controllers
{
    [Authorize] // Bắt buộc đăng nhập mới xem được vé của mình
    public class TicketsController : ApiControllerBase
    {
        // Endpoint: GET api/Tickets/mine
        // Logic: Lấy toàn bộ vé đã mua của tôi.
        [HttpGet("mine")]
        public async Task<IActionResult> GetMyTickets()
        {
            var query = new GetMyTicketsQuery();
            var result = await Mediator.Send(query);
            return Ok(result);
        }
    }
}