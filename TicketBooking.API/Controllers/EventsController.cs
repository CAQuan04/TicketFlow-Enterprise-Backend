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
        // Requirement: Organizer OR EventManager can create events.
        // Logic: Roles string is comma-separated for OR condition.
        [Authorize(Roles = Roles.Organizer + "," + Roles.EventManager)] // Allow Organizer OR EventManager.
        [HttpPost] // Define HTTP POST method.
        public async Task<IActionResult> Create(CreateEventCommand command)
        {
            // Delegate logic to MediatR handler.
            var eventId = await Mediator.Send(command);
            // Return 200 OK with ID.
            return Ok(eventId);
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