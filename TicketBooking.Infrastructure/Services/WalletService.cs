using Microsoft.EntityFrameworkCore;
using TicketBooking.Application.Common.Exceptions;
using TicketBooking.Application.Common.Interfaces;
using TicketBooking.Domain.Entities;
using TicketBooking.Domain.Enums;

namespace TicketBooking.Infrastructure.Services
{
    public class WalletService : IWalletService
    {
        private readonly IApplicationDbContext _context;

        public WalletService(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<decimal> GetBalanceAsync(Guid userId, CancellationToken cancellationToken)
        {
            var wallet = await _context.Wallets
                .AsNoTracking()
                .FirstOrDefaultAsync(w => w.UserId == userId, cancellationToken);

            return wallet?.Balance ?? 0;
        }

        public async Task ProcessTransactionAsync(
            Guid userId,
            decimal amount,
            TransactionType type,
            string referenceId,
            string description,
            CancellationToken cancellationToken)
        {
            // 1. TRANSACTION START (ACID)
            // Đảm bảo cập nhật số dư và ghi lịch sử phải thành công cùng nhau.
            using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                // 2. FETCH WALLET WITH TRACKING
                // Chúng ta cần Tracking để EF Core check RowVersion khi Save.
                var wallet = await _context.Wallets
                    .FirstOrDefaultAsync(w => w.UserId == userId, cancellationToken);

                // Auto-create wallet if not exists (Lazy Initialization).
                if (wallet == null)
                {
                    wallet = new Wallet { Id = Guid.NewGuid(), UserId = userId, Balance = 0, Currency = "VND" };
                    _context.Wallets.Add(wallet);
                    // Lưu ý: Nếu tạo mới thì chưa có RowVersion check, nhưng không sao vì Balance = 0.
                }

                // 3. BUSINESS LOGIC
                switch (type)
                {
                    case TransactionType.Deposit:
                        // Nạp tiền: Cộng vào số dư.
                        wallet.Balance += amount;
                        break;

                    case TransactionType.Payment:
                        // Thanh toán: Trừ tiền.
                        if (wallet.Balance < amount)
                        {
                            throw new InsufficientFundsException(userId);
                        }
                        wallet.Balance -= amount;
                        break;

                    case TransactionType.Refund:
                        // Hoàn tiền: Cộng lại vào số dư.
                        wallet.Balance += amount;
                        break;
                }

                // 4. AUDIT TRAIL (Ghi sổ cái)
                var history = new WalletTransaction
                {
                    Id = Guid.NewGuid(),
                    WalletId = wallet.Id,
                    Amount = amount,
                    Type = type,
                    ReferenceId = referenceId,
                    Description = description,
                    CreatedDate = DateTime.UtcNow
                };

                // Nếu Wallet mới tạo, ta cần Save Wallet trước để có ID? 
                // Không cần, EF Core thông minh tự link Navigation Property nếu ta gán object. 
                // Nhưng ở đây ta dùng WalletId, nên ok vì Wallet.Id đã được gen Guid.
                _context.WalletTransactions.Add(history);

                // 5. COMMIT & CONCURRENCY CHECK
                // SaveChanges sẽ bắn ra DbUpdateConcurrencyException nếu RowVersion không khớp.
                await _context.SaveChangesAsync(cancellationToken);

                await transaction.CommitAsync(cancellationToken);
            }
            catch (DbUpdateConcurrencyException)
            {
                // Rollback.
                await transaction.RollbackAsync(cancellationToken);
                // Ném lỗi Conflict để Client retry.
                throw new ConflictException("Wallet balance was updated by another transaction. Please retry.");
            }
            catch (Exception)
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        }
    }
}