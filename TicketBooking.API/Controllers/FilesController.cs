using Microsoft.AspNetCore.Authorization; // Needed for [Authorize].
using Microsoft.AspNetCore.Mvc; // Needed for Controller.
using TicketBooking.Application.Common.Interfaces; // Needed for IStorageService.
using TicketBooking.Domain.Constants; // Needed for Roles.

namespace TicketBooking.API.Controllers
{
    // Controller to handle File Uploads.
    public class FilesController : ApiControllerBase
    {
        private readonly IStorageService _storageService; // Inject storage service.

        public FilesController(IStorageService storageService)
        {
            _storageService = storageService; // Assign service.
        }

        // POST api/Files
        // Only Admin and Organizer can upload files (Security Requirement).
        [Authorize(Roles = Roles.Admin + "," + Roles.Organizer)]
        [HttpPost]
        public async Task<IActionResult> Upload(IFormFile file)
        {
            // Call the service to upload. 
            // We use "uploads" as a generic folder name, or could be dynamic.
            var fileUrl = await _storageService.UploadAsync(file, "uploads", CancellationToken.None);

            // Return the relative URL so the Client can verify or display it.
            // Example return: { "url": "/user-content/uploads/abc-xyz.jpg" }
            return Ok(new { Url = fileUrl });
        }
    }
}