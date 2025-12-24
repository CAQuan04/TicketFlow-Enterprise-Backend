using MailKit.Net.Smtp; // Use MailKit SmtpClient (Not System.Net.Mail).
using MailKit.Security; // For SecureSocketOptions.
using Microsoft.Extensions.Logging; // For logging errors.
using Microsoft.Extensions.Options; // For accessing settings.
using MimeKit; // For creating email messages.
using TicketBooking.Application.Common.Interfaces; // Implement the interface.
using TicketBooking.Infrastructure.Authentication; // Use EmailSettings.

namespace TicketBooking.Infrastructure.Services
{
    // Implementation of Email Service using MailKit (Production Ready).
    public class SmtpEmailService : IEmailService
    {
        // Store settings injected from DI.
        private readonly EmailSettings _emailSettings;
        // Logger to record success or failure without crashing the app.
        private readonly ILogger<SmtpEmailService> _logger;

        // Constructor injection.
        public SmtpEmailService(IOptions<EmailSettings> emailOptions, ILogger<SmtpEmailService> logger)
        {
            _emailSettings = emailOptions.Value; // Extract value from IOptions.
            _logger = logger; // Assign logger.
        }

        // Main method to send email.
        public async Task SendEmailAsync(string toEmail, string subject, string body, byte[]? attachmentData = null, string? attachmentName = null)
        {
            // Cấu hình Retry: Thử tối đa 3 lần.
            int maxRetries = 3;
            int delaySeconds = 2;

            for (int i = 0; i < maxRetries; i++)
            {
                try
                {
                    // 1. Create a new MimeMessage object.
                    var message = new MimeMessage();

                    // 2. Set the Sender details (Name and Email).
                    message.From.Add(new MailboxAddress(_emailSettings.SenderName, _emailSettings.SenderEmail));

                    // 3. Set the Recipient details.
                    message.To.Add(new MailboxAddress("", toEmail));

                    // 4. Set the Subject line.
                    message.Subject = subject;

                    // 5. Construct the Body (HTML format supported).
                    var bodyBuilder = new BodyBuilder
                    {
                        HtmlBody = body // Assign the HTML content.
                    };

                    // Nếu có file đính kèm (QR Code) thì add vào.
                    if (attachmentData != null && !string.IsNullOrEmpty(attachmentName))
                    {
                        bodyBuilder.Attachments.Add(attachmentName, attachmentData, ContentType.Parse("image/png"));
                    }

                    message.Body = bodyBuilder.ToMessageBody();



                    using var client = new SmtpClient();
                    // Timeout cho mỗi lần thử kết nối
                    client.Timeout = 5000;

                    // 7. CONNECT TO SERVER.
                    // StartTls: Upgrades an insecure connection to a secure one using TLS/SSL.
                    await client.ConnectAsync(_emailSettings.SmtpServer, _emailSettings.Port, SecureSocketOptions.StartTls);

                    // 8. AUTHENTICATE.
                    // Use the Sender Email and the App Password.
                    await client.AuthenticateAsync(_emailSettings.SenderEmail, _emailSettings.Password);

                    // 9. SEND THE MESSAGE.
                    await client.SendAsync(message);

                    // 10. DISCONNECT CLEANLY.
                    // true: Send "QUIT" command to server before closing.
                    await client.DisconnectAsync(true);

                    // Log success.
                    _logger.LogInformation("Email sent successfully to {Email}", toEmail);
                    return;
                }
                catch (Exception ex)
                {
                    // Nếu là lần thử cuối cùng thì ném lỗi ra ngoài (hoặc log error).
                    if (i == maxRetries - 1)
                    {
                        _logger.LogError(ex, $"Failed to send email to {toEmail} after {maxRetries} attempts.");
                        // Không throw exception để tránh làm chết luồng chính (Fire-and-forget), 
                        // hoặc throw nếu muốn bên gọi biết. Ở đây ta log error là đủ.
                        return;
                    }

                    // Nếu chưa phải lần cuối, chờ một chút rồi thử lại (Exponential Backoff).
                    // Lần 1: chờ 2s, Lần 2: chờ 4s...
                    _logger.LogWarning($"Email send failed. Retrying in {delaySeconds}s... (Attempt {i + 1}/{maxRetries})");
                    await Task.Delay(TimeSpan.FromSeconds(delaySeconds));
                    delaySeconds *= 2;
                }
            }
        }
    }
}