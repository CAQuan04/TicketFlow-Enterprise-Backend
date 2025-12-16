using Microsoft.Extensions.Options;
using TicketBooking.Application.Common.Interfaces.Payments;

namespace TicketBooking.Infrastructure.Payments
{
    // Implementation of VNPay Strategy.
    public class VnPayPaymentGateway : IPaymentGateway
    {
        private readonly VnPaySettings _settings;

        public VnPayPaymentGateway(IOptions<VnPaySettings> settings)
        {
            _settings = settings.Value; // Inject settings.
        }

        public string CreatePaymentUrl(decimal amount, string transactionRef, string ipAddress)
        {
            // 1. Initialize the Helper Library.
            var vnpay = new VnPayLibrary();

            // 2. Add Standard VNPay Parameters.
            vnpay.AddRequestData("vnp_Version", "2.1.0"); // API Version.
            vnpay.AddRequestData("vnp_Command", "pay"); // Command type.
            vnpay.AddRequestData("vnp_TmnCode", _settings.TmnCode); // Merchant ID.

            // 3. Add Transaction Details.
            // Amount must be multiplied by 100 (VNPay rule: 10,000 VND = 1000000).
            // Cast to long to remove decimals.
            vnpay.AddRequestData("vnp_Amount", ((long)(amount * 100)).ToString());

            vnpay.AddRequestData("vnp_CreateDate", DateTime.Now.ToString("yyyyMMddHHmmss")); // Creation Time.
            vnpay.AddRequestData("vnp_CurrCode", "VND"); // Currency.
            vnpay.AddRequestData("vnp_IpAddr", ipAddress); // Client IP Address.
            vnpay.AddRequestData("vnp_Locale", "vn"); // Language (vn/en).
            vnpay.AddRequestData("vnp_OrderInfo", "NapTienTicketFlow"); // Description.
            vnpay.AddRequestData("vnp_OrderType", "other"); // Category.
            vnpay.AddRequestData("vnp_ReturnUrl", _settings.ReturnUrl); // Callback URL.
            vnpay.AddRequestData("vnp_TxnRef", transactionRef); // Unique Reference ID (Our WalletTransactionId).

            // 4. Generate the Final URL (with Signature).
            return vnpay.CreateRequestUrl(_settings.BaseUrl, _settings.HashSecret);
        }
    }
}