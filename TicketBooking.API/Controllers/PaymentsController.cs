using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TicketBooking.API.Controllers;
using TicketBooking.Application.Features.Payments.Commands.CreateDeposit;

namespace TicketBooking.API.Controllers
{
    [Authorize]
    public class PaymentsController : ApiControllerBase
    {
        // Endpoint: POST api/Payments/Deposit
        [HttpPost("Deposit")]
        public async Task<IActionResult> Deposit([FromBody] CreateDepositCommand command)
        {
            // Call Mediator to get the Payment URL.
            var paymentUrl = await Mediator.Send(command);

            // Return the URL. The Frontend should redirect the user to this URL.
            return Ok(new { Url = paymentUrl });
        }
    }
}