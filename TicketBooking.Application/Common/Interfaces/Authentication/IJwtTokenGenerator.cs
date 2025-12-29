using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TicketBooking.Domain.Entities; // Import the User entity from Domain layer.

namespace TicketBooking.Application.Common.Interfaces.Authentication
{
    // Interface defines the contract for generating JSON Web Tokens (JWT).
    public interface IJwtTokenGenerator
    {
        // Generates a JWT string for a specific user containing their claims.
        string GenerateToken(User user);
        string GenerateRefreshToken(); // <--- Thêm hàm này
                                       // Hàm lấy Principal từ Expired Token (để biết ai đang đòi refresh)
        System.Security.Claims.ClaimsPrincipal GetPrincipalFromExpiredToken(string token);
    }
}