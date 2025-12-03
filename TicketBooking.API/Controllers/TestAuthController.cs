using Microsoft.AspNetCore.Authorization; // Needed for [Authorize] attribute.
using Microsoft.AspNetCore.Mvc; // Needed for ControllerBase.

namespace TicketBooking.API.Controllers
{
    // Define that this is an API Controller.
    [ApiController]
    // Define the route: api/TestAuth
    [Route("api/[controller]")]
    // ⚠️ IMPORTANT: The [Authorize] attribute locks this entire controller.
    // Only requests with a Valid JWT Token can access endpoints here.
    [Authorize]
    public class TestAuthController : ControllerBase
    {
        // Define a GET endpoint: api/TestAuth/Secret
        [HttpGet("secret")]
        public IActionResult GetSecretData()
        {
            // If the code reaches here, it means the Token was VALID.
            // We can access user data via the "User" property (populated by UseAuthentication).

            // Get the user ID from the "sub" claim inside the token.
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            // Return a success message with the user's ID.
            return Ok($"Congratulations! You have accessed the SECRET area. Your User ID is: {userId}");
        }
    }
}