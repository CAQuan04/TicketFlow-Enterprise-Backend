namespace TicketBooking.Domain.Enums
{
    public enum TicketStatus
    {
        Active = 0,     // Vé hợp lệ, chưa dùng.
        Used = 1,       // Vé đã check-in thành công.
        Cancelled = 2   // Vé bị hủy (hoàn tiền hoặc gian lận).
    }
}