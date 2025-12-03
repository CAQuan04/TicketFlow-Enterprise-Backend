namespace TicketBooking.Infrastructure.Authentication
{
    // Class to map email configuration from appsettings.json.
    public class EmailSettings
    {
        // The section name in JSON file.
        public const string SectionName = "EmailSettings";
        // SMTP Server address (e.g., smtp.gmail.com).
        public string SmtpServer { get; set; } = string.Empty;
        // SMTP Port (usually 587 for TLS).
        public int Port { get; set; }
        // The name displayed in the user's inbox (e.g., TicketFlow Support).
        public string SenderName { get; set; } = string.Empty;
        // The email address sending the mail.
        public string SenderEmail { get; set; } = string.Empty;
        // The App Password for authentication.
        public string Password { get; set; } = string.Empty;
    }
}