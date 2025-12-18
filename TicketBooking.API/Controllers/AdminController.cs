using Microsoft.AspNetCore.Authorization; // Dùng cho Attribute bảo mật.
using Microsoft.AspNetCore.Mvc; // Dùng cho Controller.
using TicketBooking.API.Controllers; // Kế thừa BaseController.
using TicketBooking.Application.Features.Admin.Queries.GetAdminStats; // Import Query.
using TicketBooking.Domain.Constants; // Import Roles.

namespace TicketBooking.API.Controllers
{
    // Controller dành riêng cho các tác vụ Quản trị.
    // Yêu cầu quyền Admin cho toàn bộ Controller này.
    [Authorize(Roles = Roles.Admin)]
    public class AdminController : ApiControllerBase
    {
        // Endpoint: GET api/Admin/stats/overview
        // Mục đích: Lấy số liệu tổng quan cho Dashboard.
        [HttpGet("stats/overview")]
        public async Task<IActionResult> GetOverviewStats()
        {
            // Gửi Query rỗng (vì không cần tham số đầu vào) sang Mediator.
            var result = await Mediator.Send(new GetAdminStatsQuery());

            // Trả về 200 OK cùng dữ liệu JSON.
            return Ok(result);
        }
    }
}