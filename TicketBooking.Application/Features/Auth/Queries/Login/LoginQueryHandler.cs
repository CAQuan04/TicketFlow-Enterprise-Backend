using MediatR;
using Microsoft.EntityFrameworkCore;
using TicketBooking.Application.Common.Interfaces;
using TicketBooking.Application.Common.Interfaces.Authentication;
using TicketBooking.Application.Features.Auth.DTOs;

namespace TicketBooking.Application.Features.Auth.Queries.Login
{
    public class LoginQueryHandler : IRequestHandler<LoginQuery, AuthResponse>
    {
        private readonly IApplicationDbContext _context;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IJwtTokenGenerator _jwtTokenGenerator;

        public LoginQueryHandler(
            IApplicationDbContext context,
            IPasswordHasher passwordHasher,
            IJwtTokenGenerator jwtTokenGenerator)
        {
            _context = context;
            _passwordHasher = passwordHasher;
            _jwtTokenGenerator = jwtTokenGenerator;
        }

        public async Task<AuthResponse> Handle(LoginQuery request, CancellationToken cancellationToken)
        {
            // 1. Tìm User theo Email
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email, cancellationToken);

            // 2. Kiểm tra User tồn tại
            if (user == null)
            {
                throw new Exception("Invalid credentials");
            }

            // 3. Kiểm tra Password
            bool isPasswordValid = _passwordHasher.Verify(request.Password, user.PasswordHash ?? "");

            if (!isPasswordValid)
            {
                throw new Exception("Invalid credentials");
            }

            // 4. Tạo Access Token (JWT)
            var accessToken = _jwtTokenGenerator.GenerateToken(user);

            // 5. Tạo Refresh Token (Mới)
            var refreshToken = _jwtTokenGenerator.GenerateRefreshToken();

            // 6. Cập nhật Refresh Token vào Database
            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7); // Hết hạn sau 7 ngày

            // Lưu thay đổi vào DB
            await _context.SaveChangesAsync(cancellationToken);

            // 7. Trả về cả 2 Token
            return new AuthResponse(accessToken, refreshToken);
        }
    }
}