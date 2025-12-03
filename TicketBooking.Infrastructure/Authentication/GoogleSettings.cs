namespace TicketBooking.Infrastructure.Authentication
{
    // Class to map Google configuration from appsettings.json.
    public class GoogleSettings
    {
        // The section name in the configuration file.
        public const string SectionName = "Google";
        // The Client ID provided by Google Cloud Console.
        public string ClientId { get; set; } = string.Empty;
    }
}