using Microsoft.AspNetCore.Hosting; // Needed to get the wwwroot path.
using Microsoft.AspNetCore.Http; // Needed for IFormFile.
using TicketBooking.Application.Common.Exceptions; // Import Custom Exceptions.
using TicketBooking.Application.Common.Interfaces; // Import Interface.

namespace TicketBooking.Infrastructure.FileStorage
{
    // Implementation of file storage that saves files to the server's local disk (wwwroot).
    public class LocalStorageService : IStorageService
    {
        // Environment interface to locate the 'wwwroot' folder dynamically.
        private readonly IWebHostEnvironment _webHostEnvironment;

        // Allowed extensions whitelist. NEVER allow executable extensions like .exe, .sh, .php, .asp.
        private readonly string[] _allowedExtensions = { ".jpg", ".jpeg", ".png", ".webp" };

        // Max file size in bytes (5MB). 5 * 1024 * 1024.
        private const long _maxFileSize = 5 * 1024 * 1024;

        // Constructor injection.
        public LocalStorageService(IWebHostEnvironment webHostEnvironment)
        {
            _webHostEnvironment = webHostEnvironment; // Assign environment.
        }

        public async Task<string> UploadAsync(IFormFile file, string folderName, CancellationToken cancellationToken)
        {
            // 1. INPUT VALIDATION: Check if file is null or empty.
            if (file == null || file.Length == 0)
            {
                // Throw validation exception if no file provided.
                throw new ValidationException();
            }

            // 2. SIZE VALIDATION: Check if file exceeds the limit.
            if (file.Length > _maxFileSize)
            {
                // Throw error to prevent DoS attacks via disk space exhaustion.
                throw new Exception("File size exceeds the 5MB limit.");
            }

            // 3. EXTENSION VALIDATION: Get the extension from the original filename.
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

            // Security Check: Verify if extension is in the whitelist.
            if (!_allowedExtensions.Contains(extension))
            {
                // Throw error if user tries to upload malicious scripts (e.g., shell.php).
                throw new Exception($"Invalid file type. Allowed: {string.Join(", ", _allowedExtensions)}");
            }

            // 4. PREVENT DIRECTORY TRAVERSAL & DOUBLE EXTENSION ATTACKS.
            // We DO NOT use the original 'file.FileName' to save.
            // Attackers could use filenames like "../../windows/system32/cmd.exe" or "image.php.jpg".
            // By generating a new GUID, we sanitize the filename completely.
            var newFileName = $"{Guid.NewGuid()}{extension}";

            // 5. DETERMINE STORAGE PATH.
            // Construct the path: wwwroot/user-content/{folderName}
            var webRootPath = _webHostEnvironment.WebRootPath ?? Path.Combine(_webHostEnvironment.ContentRootPath, "wwwroot");

            var uploadPath = Path.Combine(webRootPath, "user-content", folderName);

            // 6. ENSURE DIRECTORY EXISTS.
            if (!Directory.Exists(uploadPath))
            {
                Directory.CreateDirectory(uploadPath);
            }

            // 7. CONSTRUCT FULL FILE PATH.
            var filePath = Path.Combine(uploadPath, newFileName);

            // 8. SAVE FILE TO DISK.
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                // Copy the uploaded stream to the file stream asynchronously.
                await file.CopyToAsync(stream, cancellationToken);
            }

            // 9. RETURN RELATIVE URL.
            // This URL will be stored in the database (e.g., for User Avatar or Event Image).
            // Format: /user-content/events/gu-id-gu-id.jpg
            return $"/user-content/{folderName}/{newFileName}";
        }

        public Task DeleteAsync(string fileUrl, CancellationToken cancellationToken)
        {
            // Optional: Implement deletion logic if needed.
            // For now, we return completed task.
            return Task.CompletedTask;
        }
    }
}