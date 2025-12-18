using Microsoft.AspNetCore.Authorization; // Dùng cho phân quyền.
using Microsoft.AspNetCore.Mvc; // Dùng cho Controller.
using TicketBooking.API.Controllers; // Kế thừa BaseController.
using TicketBooking.Application.Features.Orders.Commands.CreateOrder; // Import Command.
using TicketBooking.Application.Features.Orders.Commands.PayOrder;
using TicketBooking.Application.Features.Orders.Queries.GetOrderById; // Import Query.
using TicketBooking.Domain.Constants; // Import Roles.

namespace TicketBooking.API.Controllers
{
    // Controller quản lý các giao dịch đặt vé.
    public class OrdersController : ApiControllerBase
    {
        // 1. ENDPOINT: TẠO ĐƠN HÀNG (POST api/Orders)
        // ⚠️ STRICT SECURITY: Chỉ Customer mới được mua vé.
        // Admin, Organizer, Inspector bị chặn để tránh gian lận/xung đột lợi ích.
        [Authorize(Roles = Roles.Customer)]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateOrderCommand command)
        {
            // 1. Gửi Command xử lý Transaction tạo đơn hàng.
            // Kết quả trả về là OrderId (Guid).
            var orderId = await Mediator.Send(command);

            // 2. Lấy thông tin chi tiết đơn hàng vừa tạo (để lấy OrderCode trả về cho Client).
            // Đây là bước phụ để đáp ứng yêu cầu UI cần hiển thị mã đơn hàng ngay lập tức.
            var orderDetails = await Mediator.Send(new GetOrderByIdQuery(orderId));

            // 3. Trả về 201 Created.
            // Header Location sẽ trỏ về endpoint GetById.
            // Body chứa thông tin đầy đủ gồm OrderId và OrderCode.
            return CreatedAtAction(
                nameof(GetById),
                new { id = orderId },
                new
                {
                    Message = "Order created successfully.",
                    Data = orderDetails
                });
        }

        // 2. ENDPOINT: XEM CHI TIẾT ĐƠN HÀNG (GET api/Orders/{id})
        // ⚠️ SECURITY: Ai tạo đơn nấy xem (Logic check Owner sẽ nằm trong Query Handler hoặc Filter sau này).
        // Tạm thời để Authorize chung.
        [Authorize]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var query = new GetOrderByIdQuery(id);
            var result = await Mediator.Send(query);
            return Ok(result);
        }

        // Endpoint: POST api/Orders/{id}/pay
        // Action: Dùng tiền trong ví để trả cho đơn hàng Pending.
        [Authorize(Roles = Roles.Customer)]
        [HttpPost("{id}/pay")]
        public async Task<IActionResult> PayWithWallet(Guid id)
        {
            var command = new PayOrderCommand(id);
            var result = await Mediator.Send(command);

            // Trả về 200 OK nếu thành công.
            return Ok(new { Message = "Payment successful. Ticket has been sent to your email." });
        }
    }
}