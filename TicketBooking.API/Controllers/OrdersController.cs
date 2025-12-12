using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TicketBooking.Application.Features.Orders.Commands.CreateOrder;

namespace TicketBooking.API.Controllers
{
    // Controller quản lý đơn hàng.
    public class OrdersController : ApiControllerBase
    {
        // Endpoint: POST api/Orders
        // Yêu cầu: Phải đăng nhập (Authorize).
        // Input: { "ticketTypeId": "...", "quantity": 2 }
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateOrderCommand command)
        {
            // Gọi xuống Handler (nơi chứa logic Transaction thần thánh vừa viết).
            var orderId = await Mediator.Send(command);

            // Trả về 201 Created.
            // Thông báo rõ ràng về thời hạn thanh toán (Business Rule).
            return CreatedAtAction(nameof(GetById), new { id = orderId }, new
            {
                OrderId = orderId,
                Message = "Order created successfully. Please proceed to payment within 10 minutes."
            });
        }

        // Placeholder endpoint để CreatedAtAction không bị lỗi.
        // Chúng ta sẽ implement chi tiết cái này sau.
        [Authorize]
        [HttpGet("{id}")]
        public IActionResult GetById(Guid id)
        {
            return Ok();
        }
    }
}