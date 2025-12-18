using MediatR;
using Microsoft.EntityFrameworkCore;
using TicketBooking.Application.Common.Interfaces;
using TicketBooking.Application.Common.Interfaces.Payments; // Import Interface mới
using TicketBooking.Domain.Entities;
using TicketBooking.Domain.Enums;

namespace TicketBooking.Application.Features.Payments.Commands.ProcessVnPayIpn
{
    public class ProcessVnPayIpnCommandHandler : IRequestHandler<ProcessVnPayIpnCommand, VnPayIpnResponseDto>
    {
        private readonly IApplicationDbContext _context;
        private readonly IVnPayValidationService _vnPayValidationService; // Inject Interface

        // Constructor thay đổi: Không inject VnPaySettings nữa
        public ProcessVnPayIpnCommandHandler(
            IApplicationDbContext context,
            IVnPayValidationService vnPayValidationService)
        {
            _context = context;
            _vnPayValidationService = vnPayValidationService;
        }

        public async Task<VnPayIpnResponseDto> Handle(ProcessVnPayIpnCommand request, CancellationToken cancellationToken)
        {
            // --- 1. SECURITY CHECK: CHECKSUM VALIDATION ---
            // Gọi Interface để kiểm tra chữ ký. Application không cần biết HashSecret là gì.
            bool checkSignature = _vnPayValidationService.ValidateSignature(request.QueryData);

            if (!checkSignature)
            {
                return new VnPayIpnResponseDto("97", "Invalid Signature");
            }

            // --- 2. GET PARAMS ---
            string vnp_TxnRef = request.QueryData["vnp_TxnRef"].ToString();
            string vnp_ResponseCode = request.QueryData["vnp_ResponseCode"].ToString();
            long vnp_Amount = Convert.ToInt64(request.QueryData["vnp_Amount"]);

            // --- 3. BUSINESS LOGIC (Giữ nguyên như cũ) ---
            if (!Guid.TryParse(vnp_TxnRef, out var transactionId))
            {
                return new VnPayIpnResponseDto("01", "Order not found");
            }

            var transaction = await _context.WalletTransactions
                .Include(t => t.Wallet)
                .FirstOrDefaultAsync(t => t.Id == transactionId, cancellationToken);

            if (transaction == null)
            {
                return new VnPayIpnResponseDto("01", "Order not found");
            }

            // Check Amount
            if ((long)(transaction.Amount * 100) != vnp_Amount)
            {
                return new VnPayIpnResponseDto("04", "Invalid Amount");
            }

            // Check Idempotency
            if (transaction.Status == TransactionStatus.Success)
            {
                return new VnPayIpnResponseDto("02", "Order already confirmed");
            }

            // Process Payment
            if (vnp_ResponseCode == "00")
            {
                using var dbTransaction = await _context.Database.BeginTransactionAsync(cancellationToken);
                try
                {
                    transaction.Status = TransactionStatus.Success;
                    transaction.Description += " | VNPay Confirmed";
                    transaction.Wallet.Balance += transaction.Amount;

                    await _context.SaveChangesAsync(cancellationToken);
                    await dbTransaction.CommitAsync(cancellationToken);

                    return new VnPayIpnResponseDto("00", "Confirm Success");
                }
                catch
                {
                    return new VnPayIpnResponseDto("99", "Unknown Error");
                }
            }
            else
            {
                transaction.Status = TransactionStatus.Failed;
                transaction.Description += $" | Failed Code: {vnp_ResponseCode}";
                await _context.SaveChangesAsync(cancellationToken);
                return new VnPayIpnResponseDto("00", "Confirm Success");
            }
        }
    }
}