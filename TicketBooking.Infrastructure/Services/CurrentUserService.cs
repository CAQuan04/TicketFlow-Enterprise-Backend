using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using TicketBooking.Application.Common.Interfaces;

namespace TicketBooking.Infrastructure.Services
{
    public class CurrentUserService : ICurrentUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CurrentUserService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        // Lấy giá trị từ Claim "sub" hoặc "nameidentifier" trong Token.
        public string? UserId => _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
    }
}