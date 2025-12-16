namespace TicketBooking.Infrastructure.Payments
{
    // Class to hold VNPay configuration from appsettings.json.
    public class VnPaySettings
    {
        public const string SectionName = "VnPay";

        public string TmnCode { get; set; } = string.Empty; // Merchant ID provided by VNPay.
        public string HashSecret { get; set; } = string.Empty; // Secret Key for signing data.
        public string BaseUrl { get; set; } = string.Empty; // Sandbox URL.
        public string ReturnUrl { get; set; } = string.Empty; // URL to handle callback after payment.
    }
}