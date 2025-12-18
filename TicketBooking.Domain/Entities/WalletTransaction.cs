using TicketBooking.Domain.Common;
using TicketBooking.Domain.Enums;

namespace TicketBooking.Domain.Entities
{
    // Bảng Audit Trail: Lưu lịch sử biến động số dư.
    // Nguyên tắc: Balance hiện tại = Tổng (Deposit + Refund) - Tổng (Payment).
    public class WalletTransaction : BaseEntity
    {
        public Guid WalletId { get; set; }
        public Wallet Wallet { get; set; } = null!;

        public decimal Amount { get; set; } // Số tiền giao dịch.

        public TransactionType Type { get; set; }

        // ID tham chiếu (Ví dụ: Mã đơn hàng OrderId, hoặc Mã giao dịch ngân hàng).
        public string ReferenceId { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        // Thêm property này vào class WalletTransaction
        public TransactionStatus Status { get; set; } = TransactionStatus.Pending;
    }
}