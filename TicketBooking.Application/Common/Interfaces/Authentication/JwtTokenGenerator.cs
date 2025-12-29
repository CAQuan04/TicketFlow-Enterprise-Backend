using Microsoft.Extensions.Options; // Namespace for IOptions pattern to access settings.
using Microsoft.IdentityModel.Tokens; // Namespace for SecurityKey and SigningCredentials.
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt; // Namespace for JWT handling classes.
using System.Linq;
using System.Security.Claims; // Namespace for creating Claims (user data).
using System.Text; // Namespace for encoding strings to bytes.
using System.Threading.Tasks;
using TicketBooking.Application.Common.Interfaces.Authentication; // Import the interface.
using TicketBooking.Domain.Entities; // Import the User domain entity.
using System.Security.Cryptography; // Cần cái này cho RNG

namespace TicketBooking.Infrastructure.Authentication
{
    // Implementation of the JWT token generator.
    public class JwtTokenGenerator : IJwtTokenGenerator
    {
        // Field to store the injected JWT settings.
        private readonly JwtSettings _jwtSettings;

        // Constructor to inject the settings using the Options pattern.
        public JwtTokenGenerator(IOptions<JwtSettings> jwtOptions)
        {
            // Assign the Value of the options to the local field.
            _jwtSettings = jwtOptions.Value;
        }

        // Method to generate the JWT token string for a user.
        public string GenerateToken(User user)
        {
            // Create the symmetric security key from the secret string in settings.
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Secret));

            // Create the signing credentials using the key and HMAC SHA256 algorithm.
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // Define the claims (payload) for the token.
            var claims = new List<Claim>
            {
                // The 'sub' (subject) claim typically holds the User ID.
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                
                // The 'jti' (JWT ID) claim creates a unique identifier for this specific token.
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                
                // The 'email' claim stores the user's email address.
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                
                // Custom claim to store the user's role (converted to string).
                new Claim("role", user.Role.ToString())
            };

            // Create the token object with all configuration parameters.
            var token = new JwtSecurityToken(
                issuer: _jwtSettings.Issuer, // The server issuing the token.
                audience: _jwtSettings.Audience, // The intended recipient of the token.
                claims: claims, // The list of user data claims.
                expires: DateTime.UtcNow.AddMinutes(_jwtSettings.ExpiryMinutes), // Set expiration time.
                signingCredentials: credentials); // Sign the token with our secret key.

            // Serialize the token object into a compact string format.
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
        public string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }
        public ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
        {
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = false, // Có thể bỏ qua check audience khi refresh
                ValidateIssuer = false,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Secret)),
                ValidateLifetime = false // QUAN TRỌNG: Không check hết hạn ở đây (vì token đã hết hạn rồi mới cần refresh)
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            // Check token format
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out SecurityToken securityToken);

            if (securityToken is not JwtSecurityToken jwtSecurityToken || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            {
                throw new SecurityTokenException("Invalid token");
            }

            return principal;
        }
    }
}