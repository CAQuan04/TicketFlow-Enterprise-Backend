using MediatR;
using Microsoft.AspNetCore.Http; // ✅ BẮT BUỘC: Để dùng IHttpContextAccessor
using System.Net.Sockets; // ✅ BẮT BUỘC: Để dùng AddressFamily
using TicketBooking.Application.Common.Interfaces;
using TicketBooking.Application.Common.Interfaces.Payments;
using TicketBooking.Domain.Entities;
using TicketBooking.Domain.Enums;

namespace TicketBooking.Application.Features.Payments.Commands.CreateDeposit
{
    public class CreateDepositCommandHandler : IRequestHandler<CreateDepositCommand, string>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUserService;
        private readonly IPaymentGateway _paymentGateway;

        // 1. KHÔI PHỤC LẠI Field này
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CreateDepositCommandHandler(
            IApplicationDbContext context,
            ICurrentUserService currentUserService,
            IPaymentGateway paymentGateway,
            // 2. KHÔI PHỤC LẠI Dependency Injection
            IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _currentUserService = currentUserService;
            _paymentGateway = paymentGateway;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<string> Handle(CreateDepositCommand request, CancellationToken cancellationToken)
        {
            var userId = Guid.Parse(_currentUserService.UserId!);

            // 1. Get User's Wallet (Create if not exists).
            var wallet = _context.Wallets.FirstOrDefault(w => w.UserId == userId);
            if (wallet == null)
            {
                wallet = new Wallet { Id = Guid.NewGuid(), UserId = userId, Balance = 0 };
                _context.Wallets.Add(wallet);
                await _context.SaveChangesAsync(cancellationToken);
            }

            // 2. Create a PENDING WalletTransaction.
            var transaction = new WalletTransaction
            {
                Id = Guid.NewGuid(),
                WalletId = wallet.Id,
                Amount = request.Amount,
                Type = TransactionType.Deposit,
                Description = "Pending Deposit via VNPay",
                ReferenceId = $"DEP-{Guid.NewGuid()}",
                CreatedDate = DateTime.UtcNow
            };

            _context.WalletTransactions.Add(transaction);
            await _context.SaveChangesAsync(cancellationToken);

            // 3. Generate VNPay URL with SMART IP DETECTION

            // Mặc định là 127.0.0.1 (đề phòng trường hợp không lấy được IP)
            string ipAddress = "127.0.0.1";

            // Logic lấy IP động và xử lý IPv6 như Sếp yêu cầu
            if (_httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress != null)
            {
                // Nếu là IPv6 (Ví dụ: ::1 trên localhost) -> Ép về IPv4
                if (_httpContextAccessor.HttpContext.Connection.RemoteIpAddress.AddressFamily == AddressFamily.InterNetworkV6)
                {
                    ipAddress = "127.0.0.1";
                }
                else
                {
                    // Nếu là IPv4 thật (Production) -> Lấy IP thật
                    ipAddress = _httpContextAccessor.HttpContext.Connection.RemoteIpAddress.ToString();
                }
            }

            var paymentUrl = _paymentGateway.CreatePaymentUrl(request.Amount, transaction.Id.ToString(), ipAddress);

            return paymentUrl;
        }
    }
}