namespace TicketBooking.Domain.Enums
{
    public enum TransactionStatus
    {
        Pending = 0, // Đang chờ thanh toán
        Success = 1, // Thành công (Đã cộng tiền)
        Failed = 2   // Thất bại
    }
}