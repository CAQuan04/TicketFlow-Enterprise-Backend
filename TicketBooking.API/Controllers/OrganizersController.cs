using Microsoft.AspNetCore.Authorization; // Dùng cho phân quyền.
using Microsoft.AspNetCore.Mvc; // Dùng cho Controller.
using TicketBooking.API.Controllers; // Kế thừa BaseController.
using TicketBooking.Application.Features.Organizers.Queries.GetEventStats; // Import Query.
using TicketBooking.Domain.Constants; // Import Roles Constant.

namespace TicketBooking.API.Controllers
{
    // Controller dành cho các nghiệp vụ của Ban Tổ Chức (Organizer).
    // Yêu cầu Role Organizer cho toàn bộ Controller (hoặc override từng method).
    [Authorize(Roles = Roles.Organizer)]
    public class OrganizersController : ApiControllerBase
    {
        // Endpoint: GET api/Organizers/events/{id}/stats
        // Mục đích: Lấy dữ liệu báo cáo để vẽ biểu đồ cho một sự kiện cụ thể.
        [HttpGet("events/{id}/stats")]
        public async Task<IActionResult> GetEventStats(Guid id)
        {
            // Tạo Query object với EventId lấy từ URL.
            var query = new GetEventStatsQuery(id);

            // Gửi sang Mediator xử lý logic.
            // Handler sẽ tự check xem User hiện tại có phải chủ nhân Event không.
            var result = await Mediator.Send(query);

            // Trả về 200 OK và dữ liệu JSON.
            return Ok(result);
        }
    }
}