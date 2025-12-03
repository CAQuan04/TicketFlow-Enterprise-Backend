using Microsoft.AspNetCore.Authorization; // Import Authorization attributes.
using Microsoft.AspNetCore.Mvc; // Import MVC logic.
using TicketBooking.API.Controllers; // Import Base Controller.
using TicketBooking.Application.Features.Venues.Commands.CreateVenue; // Import Command.
using TicketBooking.Domain.Constants; // Import Roles constants.

namespace TicketBooking.API.Controllers
{
    // Controller for Venue management endpoints.
    public class VenuesController : ApiControllerBase
    {
        // Endpoint: POST api/Venues
        // Requirement: ONLY ADMIN can create venues.
        [Authorize(Roles = Roles.Admin)] // Secure this endpoint for Admin role only.
        [HttpPost] // Define HTTP POST method.
        public async Task<IActionResult> Create(CreateVenueCommand command)
        {
            // Delegate logic to MediatR handler.
            var venueId = await Mediator.Send(command);
            // Return 200 OK with ID.
            return Ok(venueId);
        }

        // Example: GET api/Venues
        // Requirement: Public access (Everyone can see venues).
        [AllowAnonymous] // Explicitly allow unauthenticated access.
        [HttpGet] // Define HTTP GET method.
        public IActionResult GetAll()
        {
            // Placeholder for Get logic.
            return Ok("Public Venue List");
        }
    }
}