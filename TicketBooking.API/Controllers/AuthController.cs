using Microsoft.AspNetCore.Mvc; // Needed for Controller attributes.
using TicketBooking.Application.Features.Auth.Commands.VerifyEmail;
using TicketBooking.Application.Features.Auth.Queries.Login; // Needed for LoginQuery.
using TicketBooking.Application.Features.Users.Commands.CreateUser; // Needed for CreateUserCommand (Register).

namespace TicketBooking.API.Controllers
{
    // Inherit from the Base Controller to access the Mediator instance.
    public class AuthController : ApiControllerBase
    {
        // Endpoint for User Login.
        // POST: api/Auth/Login
        [HttpPost("Login")]
        public async Task<IActionResult> Login(LoginQuery query)
        {
            // Send the LoginQuery to the Handler via MediatR.
            // The handler will return a JWT string if successful.
            var token = await Mediator.Send(query);

            // Return HTTP 200 OK with the token in the response body.
            return Ok(token);
        }

        // Endpoint for User Registration (Moved logic here for better organization, optional).
        // POST: api/Auth/Register
        [HttpPost("Register")]
        public async Task<IActionResult> Register(CreateUserCommand command)
        {
            // Reuse the existing CreateUser command logic.
            var userId = await Mediator.Send(command);

            // Return HTTP 200 OK with the new User ID.
            return Ok(userId);
        }

        [HttpPost("Verify")]
        public async Task<IActionResult> Verify(VerifyEmailCommand command)
        {
            var result = await Mediator.Send(command);
            return Ok(result);
        }
    }
}