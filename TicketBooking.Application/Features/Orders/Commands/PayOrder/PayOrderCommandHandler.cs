using MediatR;
using Microsoft.EntityFrameworkCore;
using TicketBooking.Application.Common.Exceptions;
using TicketBooking.Application.Common.Interfaces;
using TicketBooking.Application.Features.Orders.Events; // Import Event.
using TicketBooking.Domain.Entities;
using TicketBooking.Domain.Enums;

namespace TicketBooking.Application.Features.Orders.Commands.PayOrder
{
    public class PayOrderCommandHandler : IRequestHandler<PayOrderCommand, bool>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUserService;
        private readonly IPublisher _publisher; // Dùng để bắn Event.

        public PayOrderCommandHandler(
            IApplicationDbContext context,
            ICurrentUserService currentUserService,
            IPublisher publisher)
        {
            _context = context;
            _currentUserService = currentUserService;
            _publisher = publisher;
        }

        public async Task<bool> Handle(PayOrderCommand request, CancellationToken cancellationToken)
        {
            var userId = Guid.Parse(_currentUserService.UserId!);

            // 1. TRANSACTION START
            using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
            try
            {
                // 2. GET ORDER & WALLET (Eager Load)
                var order = await _context.Orders
                    .Include(o => o.Tickets) // Load vé để lát nữa sinh code.
                    .FirstOrDefaultAsync(o => o.Id == request.OrderId, cancellationToken);

                var wallet = await _context.Wallets
                    .FirstOrDefaultAsync(w => w.UserId == userId, cancellationToken);

                // 3. VALIDATION
                if (order == null) throw new NotFoundException(nameof(Order), request.OrderId);
                if (order.UserId != userId) throw new UnauthorizedAccessException("Not your order.");
                if (order.Status == OrderStatus.Paid) return true; // Idempotency check.
                if (order.Status == OrderStatus.Cancelled) throw new Exception("Order expired.");

                if (wallet == null || wallet.Balance < order.TotalAmount)
                {
                    throw new Exception("Insufficient wallet balance. Please Deposit first.");
                }

                // 4. PROCESS PAYMENT (Trừ tiền ví)
                wallet.Balance -= order.TotalAmount;

                // Ghi lịch sử ví (Audit).
                _context.WalletTransactions.Add(new WalletTransaction
                {
                    Id = Guid.NewGuid(),
                    WalletId = wallet.Id,
                    Amount = order.TotalAmount,
                    Type = TransactionType.Payment,
                    ReferenceId = order.OrderCode,
                    Description = $"Payment for Order {order.OrderCode}",
                    CreatedDate = DateTime.UtcNow,
                    Status = TransactionStatus.Success
                });

                // 5. UPDATE ORDER STATUS
                order.Status = OrderStatus.Paid;

                // 6. GENERATE TICKET CODES (Unique)
                // Đây là thời điểm an toàn nhất để sinh mã vé. Tiền đã trao, cháo mới múc.
                foreach (var ticket in order.Tickets)
                {
                    // Format: EVT-{Random8Chars}
                    ticket.TicketCode = $"EVT-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}";
                }

                await _context.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                // 7. PUBLISH EVENT (FIRE AND FORGET LOGIC)
                // Bắn sự kiện "Đã trả tiền". Handler gửi mail sẽ bắt được cái này.
                // Việc gửi mail có thể chậm, nhưng Transaction thanh toán phải xong nhanh.
                await _publisher.Publish(new OrderPaidNotification(order.Id), cancellationToken);

                return true;
            }
            catch
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        }
    }
}