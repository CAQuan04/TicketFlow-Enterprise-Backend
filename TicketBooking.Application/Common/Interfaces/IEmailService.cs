namespace TicketBooking.Application.Common.Interfaces
{
    // Interface for sending emails abstraction.
    public interface IEmailService
    {
        // Asynchronous method to send an email.
        Task SendEmailAsync(string toEmail, string subject, string body, byte[]? attachmentData = null, string? attachmentName = null);
    }
}