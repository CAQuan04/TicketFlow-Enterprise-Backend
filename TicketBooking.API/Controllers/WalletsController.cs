using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TicketBooking.API.Controllers;
using TicketBooking.Application.Common.Interfaces;
using TicketBooking.Domain.Constants;
using TicketBooking.Domain.Enums;

namespace TicketBooking.API.Controllers
{
    [Authorize] // Phải đăng nhập mới có ví.
    public class WalletsController : ApiControllerBase
    {
        private readonly IWalletService _walletService;
        private readonly ICurrentUserService _currentUserService;

        public WalletsController(IWalletService walletService, ICurrentUserService currentUserService)
        {
            _walletService = walletService;
            _currentUserService = currentUserService;
        }

        // GET api/Wallets/Balance
        [HttpGet("Balance")]
        public async Task<IActionResult> GetBalance()
        {
            var userId = Guid.Parse(_currentUserService.UserId!);
            var balance = await _walletService.GetBalanceAsync(userId, CancellationToken.None);
            return Ok(new { Balance = balance, Currency = "VND" });
        }

        // POST api/Wallets/Deposit
        // Giả lập nạp tiền (Trong thực tế API này sẽ được gọi bởi Webhook của Cổng thanh toán).
        [HttpPost("Deposit")]
        public async Task<IActionResult> Deposit([FromBody] DepositRequest request)
        {
            var userId = Guid.Parse(_currentUserService.UserId!);

            await _walletService.ProcessTransactionAsync(
                userId,
                request.Amount,
                TransactionType.Deposit,
                $"TOPUP-{Guid.NewGuid()}",
                "Nạp tiền vào ví",
                CancellationToken.None);

            return Ok(new { Message = "Deposit successful." });
        }
    }

    public record DepositRequest(decimal Amount);
}