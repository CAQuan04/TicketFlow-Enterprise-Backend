using Microsoft.AspNetCore.Authorization; // Import Authorization attributes.
using Microsoft.AspNetCore.Mvc; // Import MVC logic.
using TicketBooking.API.Controllers; // Import Base Controller.
using TicketBooking.Application.Features.Events.Commands.CreateEvent; // Import Command.
using TicketBooking.Domain.Constants; // Import Roles constants.

namespace TicketBooking.API.Controllers
{
    // Controller for Event management endpoints.
    public class EventsController : ApiControllerBase
    {
        // Endpoint: POST api/Events
        // Quyền hạn: Chỉ Organizer mới được phép tạo sự kiện.
        [Authorize(Roles = Roles.Organizer)]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateEventCommand command)
        {
            // Gửi lệnh xử lý qua Mediator.
            // Toàn bộ logic validate, check database, transaction đều nằm trong Handler.
            var eventId = await Mediator.Send(command);

            // Trả về 201 Created (Chuẩn RESTful hơn là 200 OK khi tạo mới).
            // Kèm theo ID của tài nguyên vừa tạo.
            return CreatedAtAction(nameof(GetById), new { id = eventId }, eventId);
            // Lưu ý: Sếp cần có endpoint GetById để dùng CreatedAtAction, nếu chưa có thì dùng return Ok(eventId);
        }

        // Placeholder cho GetById để code trên không bị lỗi biên dịch
        [HttpGet("{id}")]
        [AllowAnonymous]
        public IActionResult GetById(Guid id)
        {
            return Ok();
        }

        // Example: DELETE api/Events/{id}
        // Requirement: Admin OR Organizer can delete events.
        [Authorize(Roles = Roles.Admin + "," + Roles.Organizer)] // Allow Admin OR Organizer.
        [HttpDelete("{id}")] // Define HTTP DELETE method.
        public IActionResult Delete(Guid id)
        {
            // Placeholder for Delete logic.
            return Ok($"Event {id} deleted");
        }

        // Example: GET api/Events
        // Requirement: Public access.
        [AllowAnonymous] // Allow everyone.
        [HttpGet] // Define HTTP GET method.
        public IActionResult GetAll()
        {
            // Placeholder for Get logic.
            return Ok("Public Event List");
        }
    }
}