using Microsoft.AspNetCore.Authorization; // Thư viện xác thực quyền truy cập.
using Microsoft.AspNetCore.Mvc; // Thư viện xây dựng API Controller.
using TicketBooking.API.Controllers; // Class cha ApiControllerBase.
// Import các Features (Commands & Queries) đã xây dựng:
using TicketBooking.Application.Features.Events.Commands.ApproveEvent;
using TicketBooking.Application.Features.Events.Commands.CancelEvent; // Mới thêm
using TicketBooking.Application.Features.Events.Commands.CreateEvent;
using TicketBooking.Application.Features.Events.Commands.UpdateEvent; // Mới thêm
using TicketBooking.Application.Features.Events.Queries.GetEventDetail;
using TicketBooking.Application.Features.Events.Queries.GetEventsList;
using TicketBooking.Domain.Constants; // Hằng số Roles.

namespace TicketBooking.API.Controllers
{
    // Controller quản lý toàn bộ vòng đời của Sự Kiện (Event Lifecycle).
    public class EventsController : ApiControllerBase
    {
        // =================================================================
        // 1. PUBLIC READ APIs (SEARCH & DETAIL) - Hiệu năng cao
        // =================================================================

        // Endpoint: GET api/Events
        // Chức năng: Tìm kiếm, lọc, phân trang sự kiện.
        // Quyền hạn: Public (Ai cũng xem được).
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAll([FromQuery] GetEventsListQuery query)
        {
            // [FromQuery]: Map tham số từ URL (VD: ?searchTerm=Rock&pageIndex=1) vào object Query.
            // Mediator gửi đến Handler -> Handler dùng Dynamic LINQ & AsNoTracking để query DB.
            var result = await Mediator.Send(query);

            // Trả về 200 OK kèm danh sách phân trang (PagedResult).
            return Ok(result);
        }

        // Endpoint: GET api/Events/{id}
        // Chức năng: Xem chi tiết một sự kiện cụ thể.
        // Quyền hạn: Public.
        [AllowAnonymous]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            // Tạo Query object.
            var query = new GetEventDetailQuery(id);

            // Gửi qua Mediator. Handler sẽ ném lỗi NotFound nếu không tìm thấy hoặc chưa Published.
            var result = await Mediator.Send(query);

            // Trả về 200 OK kèm dữ liệu chi tiết.
            return Ok(result);
        }

        // =================================================================
        // 2. ORGANIZER APIs (CREATE & UPDATE) - Yêu cầu chính chủ
        // =================================================================

        // Endpoint: POST api/Events
        // Chức năng: Tạo sự kiện mới (Status mặc định là Draft).
        // Quyền hạn: Chỉ Organizer.
        [Authorize(Roles = Roles.Organizer)]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateEventCommand command)
        {
            // Gửi lệnh tạo. Handler sẽ validate Capacity và lưu Transactional.
            var eventId = await Mediator.Send(command);

            // Trả về 201 Created kèm Header 'Location' trỏ đến API xem chi tiết.
            return CreatedAtAction(nameof(GetById), new { id = eventId }, eventId);
        }

        // Endpoint: PUT api/Events/{id}
        // Chức năng: Cập nhật thông tin sự kiện.
        // Quyền hạn: Chỉ Organizer (Handler sẽ check xem có phải chủ sự kiện không).
        [Authorize(Roles = Roles.Organizer)]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateEventCommand command)
        {
            // Bảo vệ tính toàn vẹn: ID trên URL phải khớp với ID trong Body.
            if (id != command.EventId)
            {
                return BadRequest("ID mismatch: URL ID does not match Body ID.");
            }

            // Gửi lệnh Update. 
            // Handler sẽ check: 
            // 1. Sự kiện có tồn tại không?
            // 2. Người gọi có phải chủ nhân (CreatedBy) không? -> Nếu không throw Unauthorized.
            // 3. Sự kiện có bị hủy chưa?
            await Mediator.Send(command);

            // Trả về 204 No Content (Update thành công, không cần trả dữ liệu).
            return NoContent();
        }

        // =================================================================
        // 3. MODERATION & MANAGEMENT APIs (APPROVE & CANCEL)
        // =================================================================

        // Endpoint: PUT api/Events/{id}/approve
        // Chức năng: Duyệt sự kiện (Chuyển Draft -> Published).
        // Quyền hạn: Chỉ ADMIN (Strict Security).
        [Authorize(Roles = Roles.Admin)]
        [HttpPut("{id}/approve")]
        public async Task<IActionResult> Approve(Guid id)
        {
            // Gửi lệnh Approve. Handler check trạng thái hiện tại để tránh duyệt 2 lần.
            await Mediator.Send(new ApproveEventCommand(id));

            // Trả về 204 No Content.
            return NoContent();
        }

        // Endpoint: DELETE api/Events/{id}
        // Chức năng: Hủy sự kiện (Chuyển Status -> Cancelled - Soft Delete).
        // Quyền hạn: Organizer (Chính chủ) HOẶC Admin (Quyền quản trị).
        [Authorize(Roles = Roles.Organizer + "," + Roles.Admin)]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Cancel(Guid id)
        {
            // Gửi lệnh Cancel.
            // Handler sẽ check quyền sở hữu (Nếu là Organizer) hoặc cho phép luôn (Nếu là Admin).
            await Mediator.Send(new CancelEventCommand(id));

            // Trả về 204 No Content.
            return NoContent();
        }
    }
}