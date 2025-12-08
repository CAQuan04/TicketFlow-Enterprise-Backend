using Microsoft.AspNetCore.Http; // Import to use IFormFile.

namespace TicketBooking.Application.Common.Interfaces
{
    // Abstraction for File Storage.
    // Allows switching between Local Storage, AWS S3, or Azure Blob Storage without changing the Controller.
    public interface IStorageService
    {
        // Uploads a file and returns the public URL.
        Task<string> UploadAsync(IFormFile file, string folderName, CancellationToken cancellationToken);

        // Deletes a file by its URL (Optional for now, but good for cleanup).
        Task DeleteAsync(string fileUrl, CancellationToken cancellationToken);
    }
}