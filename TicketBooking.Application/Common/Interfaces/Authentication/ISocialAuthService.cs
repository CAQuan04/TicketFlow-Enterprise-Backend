using TicketBooking.Application.Common.DTOs; // Import DTO.

namespace TicketBooking.Application.Common.Interfaces.Authentication
{
    // Interface defining the Strategy for Social Authentication.
    public interface ISocialAuthService
    {
        // Method to validate the token with the provider and return standardized user info.
        Task<SocialUserDto> ValidateTokenAsync(string token, CancellationToken cancellationToken);
    }
}