using QRCoder; // Thư viện tạo QR.
using TicketBooking.Application.Common.Interfaces;

namespace TicketBooking.Infrastructure.Services
{
    public class QrCodeService : IQrCodeService
    {
        public byte[] GenerateQrCode(string ticketCode)
        {
            // 1. Khởi tạo Generator.
            using var qrGenerator = new QRCodeGenerator();

            // 2. Tạo Data cho QR (Level Q - sửa lỗi trung bình).
            using var qrCodeData = qrGenerator.CreateQrCode(ticketCode, QRCodeGenerator.ECCLevel.Q);

            // 3. Render ra ảnh PngByteQRCode (nhẹ hơn Bitmap, không phụ thuộc System.Drawing trên Linux/Docker).
            using var qrCode = new PngByteQRCode(qrCodeData);

            // 4. Lấy mảng byte (20 pixel mỗi module - độ phân giải cao).
            return qrCode.GetGraphic(20);
        }
    }
}