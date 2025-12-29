using MediatR;
using Microsoft.EntityFrameworkCore;
using TicketBooking.Application.Common.Exceptions;
using TicketBooking.Application.Common.Interfaces;
using TicketBooking.Application.Common.Interfaces.Authentication;
using TicketBooking.Application.Features.Auth.DTOs;

namespace TicketBooking.Application.Features.Auth.Commands.RefreshToken
{
    public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, AuthResponse>
    {
        private readonly IApplicationDbContext _context;
        private readonly IJwtTokenGenerator _jwtGenerator;

        public RefreshTokenCommandHandler(IApplicationDbContext context, IJwtTokenGenerator jwtGenerator)
        {
            _context = context;
            _jwtGenerator = jwtGenerator;
        }

        public async Task<AuthResponse> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
        {
            // 1. Lấy thông tin user từ cái Access Token đã hết hạn
            var principal = _jwtGenerator.GetPrincipalFromExpiredToken(request.AccessToken);
            var userId = principal.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            if (userId == null) throw new ValidationException("Invalid Token");

            // 2. Tìm User trong DB
            var user = await _context.Users.FindAsync(new object[] { Guid.Parse(userId) }, cancellationToken);

            // 3. Kiểm tra Refresh Token có khớp và còn hạn không
            if (user == null || user.RefreshToken != request.RefreshToken || user.RefreshTokenExpiryTime <= DateTime.UtcNow)
            {
                throw new ValidationException("Invalid or expired refresh token");
            }

            // 4. Tạo cặp Token mới
            var newAccessToken = _jwtGenerator.GenerateToken(user);
            var newRefreshToken = _jwtGenerator.GenerateRefreshToken();

            // 5. Cập nhật vào DB (Thu hồi cái cũ, cấp cái mới)
            user.RefreshToken = newRefreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);

            await _context.SaveChangesAsync(cancellationToken);

            return new AuthResponse(newAccessToken, newRefreshToken);
        }
    }
}