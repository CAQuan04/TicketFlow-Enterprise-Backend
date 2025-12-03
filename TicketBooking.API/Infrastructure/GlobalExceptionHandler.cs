using Microsoft.AspNetCore.Diagnostics; // Needed for IExceptionHandler.
using Microsoft.AspNetCore.Mvc; // Needed for ProblemDetails.
using TicketBooking.Application.Common.Exceptions; // Import our custom exceptions.

namespace TicketBooking.API.Infrastructure
{
    // Global handler to catch exceptions and convert them into standard HTTP responses.
    public class GlobalExceptionHandler : IExceptionHandler
    {
        // Inject Logger to record the error details.
        private readonly ILogger<GlobalExceptionHandler> _logger;

        public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
        {
            _logger = logger; // Assign logger.
        }

        // The core method to handle exceptions asynchronously.
        public async ValueTask<bool> TryHandleAsync(
            HttpContext httpContext,
            Exception exception,
            CancellationToken cancellationToken)
        {
            // Log the exception message and type for debugging purposes.
            _logger.LogError(exception, "Exception occurred: {Message}", exception.Message);

            // Create a standardized ProblemDetails object (RFC 7807).
            var problemDetails = new ProblemDetails
            {
                Instance = httpContext.Request.Path // The API endpoint where the error occurred.
            };

            // Switch on the type of exception to determine the Status Code and Title.
            switch (exception)
            {
                case ValidationException validationException:
                    // 400 Bad Request for Validation Errors.
                    httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
                    problemDetails.Title = "Validation Failure";
                    problemDetails.Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1";
                    // Add the detailed validation errors dictionary to the response extensions.
                    problemDetails.Extensions.Add("errors", validationException.Errors);
                    break;

                case NotFoundException notFoundException:
                    // 404 Not Found for missing resources.
                    httpContext.Response.StatusCode = StatusCodes.Status404NotFound;
                    problemDetails.Title = "Resource Not Found";
                    problemDetails.Type = "https://tools.ietf.org/html/rfc7231#section-6.5.4";
                    problemDetails.Detail = notFoundException.Message;
                    break;

                default:
                    // 500 Internal Server Error for any unhandled/unexpected exceptions.
                    httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
                    problemDetails.Title = "Internal Server Error";
                    problemDetails.Type = "https://tools.ietf.org/html/rfc7231#section-6.6.1";
                    // In production, we usually hide the raw exception message for security.
                    problemDetails.Detail = "An unexpected error occurred.";
                    break;
            }

            // Write the ProblemDetails object as a JSON response to the client.
            await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

            // Return true to indicate that the exception has been handled effectively.
            return true;
        }
    }
}