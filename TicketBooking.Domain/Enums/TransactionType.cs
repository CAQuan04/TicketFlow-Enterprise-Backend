namespace TicketBooking.Domain.Enums
{
    public enum TransactionType
    {
        Deposit = 1,    // Nạp tiền vào ví
        Payment = 2,    // Thanh toán đơn hàng
        Refund = 3      // Hoàn tiền (khi hủy vé)
    }
}