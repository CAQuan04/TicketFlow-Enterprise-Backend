using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using TicketBooking.Application.Common.Interfaces.Payments;

namespace TicketBooking.Infrastructure.Payments
{
    public class VnPayValidationService : IVnPayValidationService
    {
        private readonly VnPaySettings _settings;

        public VnPayValidationService(IOptions<VnPaySettings> settings)
        {
            _settings = settings.Value;
        }

        public bool ValidateSignature(IQueryCollection queryData)
        {
            var vnpay = new VnPayLibrary();

            // 1. Đổ dữ liệu từ IQueryCollection vào thư viện
            foreach (var (key, value) in queryData)
            {
                // Chỉ lấy các tham số bắt đầu bằng vnp_ và KHÔNG lấy vnp_SecureHash
                if (!string.IsNullOrEmpty(key) && key.StartsWith("vnp_") && key != "vnp_SecureHash")
                {
                    vnpay.AddRequestData(key, value.ToString());
                }
            }

            // 2. Lấy chữ ký do VNPay gửi về
            string vnp_SecureHash = queryData["vnp_SecureHash"].ToString();

            // 3. Gọi thư viện để kiểm tra
            return vnpay.ValidateSignature(vnp_SecureHash, _settings.HashSecret);
        }
    }
}