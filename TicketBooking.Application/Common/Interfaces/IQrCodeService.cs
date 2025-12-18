namespace TicketBooking.Application.Common.Interfaces
{
    public interface IQrCodeService
    {
        // Chuyển đổi chuỗi mã vé thành mảng byte hình ảnh (PNG).
        byte[] GenerateQrCode(string ticketCode);
    }
}