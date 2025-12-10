using Microsoft.AspNetCore.Authorization; // Import Authorization attributes.
using Microsoft.AspNetCore.Mvc; // Import MVC logic.
using TicketBooking.API.Controllers; // Import Base Controller.
using TicketBooking.Application.Features.Events.Commands.ApproveEvent; // Import Approve Command.
using TicketBooking.Application.Features.Events.Commands.CreateEvent; // Import Create Command.
using TicketBooking.Application.Features.Events.Queries.GetEventDetail; // Import Get Detail Query.
using TicketBooking.Application.Features.Events.Queries.GetEventsList;
using TicketBooking.Domain.Constants; // Import Roles constants.

namespace TicketBooking.API.Controllers
{
    // Controller quản lý các tính năng liên quan đến Sự Kiện (Events).
    public class EventsController : ApiControllerBase
    {
        // 1. TẠO SỰ KIỆN (CREATE)
        // Endpoint: POST api/Events
        // Quyền hạn: Chỉ Organizer mới được phép tạo sự kiện.
        [Authorize(Roles = Roles.Organizer)]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateEventCommand command)
        {
            // Gửi lệnh xử lý qua Mediator.
            // Logic: Validate -> Check Venue -> Check Capacity -> Save Transaction.
            var eventId = await Mediator.Send(command);

            // Trả về 201 Created kèm theo Header 'Location' trỏ đến API xem chi tiết.
            // Điều này giúp Frontend biết ngay link để xem event vừa tạo.
            return CreatedAtAction(nameof(GetById), new { id = eventId }, eventId);
        }

        // 2. XEM CHI TIẾT SỰ KIỆN (READ DETAIL) - ĐÃ CẬP NHẬT
        // Endpoint: GET api/Events/{id}
        // Quyền hạn: Public (Ai cũng xem được, không cần Token).
        [AllowAnonymous]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            // Tạo Query object chứa ID sự kiện cần lấy.
            var query = new GetEventDetailQuery(id);

            // Gửi qua Mediator.
            // Handler sẽ dùng AsNoTracking để tối ưu hiệu năng đọc (High Performance).
            // Nếu Event là Draft hoặc không tồn tại, Handler sẽ ném lỗi 404.
            var result = await Mediator.Send(query);

            // Trả về 200 OK cùng dữ liệu chi tiết (Event + Venue + TicketTypes).
            return Ok(result);
        }

        // 3. DUYỆT SỰ KIỆN (APPROVE)
        // Endpoint: PUT api/Events/{id}/approve
        // Quyền hạn: Chỉ ADMIN mới được duyệt (Strict Security).
        [Authorize(Roles = Roles.Admin)]
        [HttpPut("{id}/approve")]
        public async Task<IActionResult> Approve(Guid id)
        {
            // Tạo Command object thủ công từ ID trên URL.
            var command = new ApproveEventCommand(id);

            // Gửi qua Mediator.
            // Handler sẽ check xem sự kiện đã Published chưa để tránh xử lý thừa.
            await Mediator.Send(command);

            // Trả về 204 No Content (Thành công nhưng không cần trả về dữ liệu gì).
            return NoContent();
        }

        // 4. XÓA SỰ KIỆN (DELETE) - Placeholder
        // Endpoint: DELETE api/Events/{id}
        // Quyền hạn: Admin hoặc Organizer.
        [Authorize(Roles = Roles.Admin + "," + Roles.Organizer)]
        [HttpDelete("{id}")]
        public IActionResult Delete(Guid id)
        {
            // (Chưa implement logic xóa thật, tạm thời trả về thông báo).
            // Sếp có thể yêu cầu implement DeleteEventCommand sau này.
            return Ok($"Event {id} deleted (Placeholder)");
        }
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAll([FromQuery] GetEventsListQuery query)
        {
            // [FromQuery]: Tự động lấy các tham số từ URL (Query String) map vào properties của GetEventsListQuery.
            // MediatR sẽ tự tìm Handler -> Chạy Validator -> Chạy Logic Query -> Trả kết quả.
            var result = await Mediator.Send(query);

            return Ok(result);
        }
    }
}