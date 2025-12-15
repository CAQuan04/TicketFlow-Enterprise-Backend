using TicketBooking.Domain.Enums;

namespace TicketBooking.Application.Common.Interfaces
{
    public interface IWalletService
    {
        // Hàm xử lý giao dịch tập trung (Nạp, Trừ, Hoàn).
        Task ProcessTransactionAsync(Guid userId, decimal amount, TransactionType type, string referenceId, string description, CancellationToken cancellationToken);

        // Hàm lấy số dư.
        Task<decimal> GetBalanceAsync(Guid userId, CancellationToken cancellationToken);
    }
}