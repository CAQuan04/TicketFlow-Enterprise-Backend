using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TicketBooking.API.Controllers;
using TicketBooking.Application.Features.Payments.Commands.CreateDeposit;
using TicketBooking.Application.Features.Payments.Commands.ProcessVnPayIpn;

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

        // Endpoint: GET api/Payments/Callback
        // QUAN TRỌNG: Phải có [AllowAnonymous] vì lúc VNPay trả về, trình duyệt không mang theo Token.
        [AllowAnonymous]
        [HttpGet("Callback")]
        public async Task<IActionResult> Callback()
        {
            // 1. Lấy toàn bộ tham số từ URL
            var queryCollection = Request.Query;

            // 2. Gửi sang Mediator để xử lý (Check chữ ký, cộng tiền...)
            var command = new ProcessVnPayIpnCommand(queryCollection);
            var response = await Mediator.Send(command);

            // 3. Trả về kết quả JSON để Sếp nhìn thấy trên màn hình
            return Ok(response);
        }
    }
}