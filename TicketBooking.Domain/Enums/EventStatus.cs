namespace TicketBooking.Domain.Enums
{
    // Định nghĩa các trạng thái của một sự kiện trong vòng đời hệ thống.
    public enum EventStatus
    {
        Draft = 0,      // Nháp (Chưa hiển thị cho khách mua).
        Published = 1,  // Đã công bố (Khách có thể xem và mua).
        Cancelled = 2,  // Đã hủy (Ngưng bán, hoàn tiền - logic sau này).
        Completed = 3   // Đã diễn ra xong.
    }
}