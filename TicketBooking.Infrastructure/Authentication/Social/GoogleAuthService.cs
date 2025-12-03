using Google.Apis.Auth; // Import Google Auth library.
using Microsoft.Extensions.Options; // Import Options pattern.
using TicketBooking.Application.Common.DTOs; // Import DTO.
using TicketBooking.Application.Common.Exceptions; // Import Custom Exceptions.
using TicketBooking.Application.Common.Interfaces.Authentication; // Import Interface.

namespace TicketBooking.Infrastructure.Authentication.Social
{
    // Implementation of Social Auth Strategy specifically for Google.
    public class GoogleAuthService : ISocialAuthService
    {
        // Store the Google settings (ClientId).
        private readonly GoogleSettings _googleSettings;

        // Constructor injection.
        public GoogleAuthService(IOptions<GoogleSettings> googleSettings)
        {
            _googleSettings = googleSettings.Value; // Extract settings.
        }

        // Validate the Google ID Token.
        public async Task<SocialUserDto> ValidateTokenAsync(string token, CancellationToken cancellationToken)
        {
            try
            {
                // 1. Configure Validation Settings.
                var settings = new GoogleJsonWebSignature.ValidationSettings
                {
                    // SECURITY CRITICAL: We must ensure the token was issued FOR US.
                    // If we don't check this, a token generated for another app could be used here (Token Reuse Attack).
                    Audience = new List<string> { _googleSettings.ClientId }
                };

                // 2. Validate the Token with Google's Servers.
                // This method throws an exception if the signature is invalid or expired.
                var payload = await GoogleJsonWebSignature.ValidateAsync(token, settings);

                // 3. Map Google's Payload to our Standard DTO.
                return new SocialUserDto
                {
                    Email = payload.Email, // Email is verified by Google.
                    FirstName = payload.GivenName, // First Name.
                    LastName = payload.FamilyName, // Last Name.
                    AvatarUrl = payload.Picture, // Profile Image URL.
                    ProviderUserId = payload.Subject // The unique User ID in Google's system.
                };
            }
            catch (InvalidJwtException ex)
            {
                // If validation fails (expired, wrong signature), throw our custom ValidationException.
                // This ensures the API returns 400 Bad Request, not 500.
                throw new ValidationException();
            }
        }
    }
}