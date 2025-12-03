namespace TicketBooking.Application.Common.DTOs
{
    // Data Transfer Object to standardize user info returned from ANY social provider.
    public class SocialUserDto
    {
        // The email address from the provider.
        public string Email { get; set; } = string.Empty;
        // The user's first name.
        public string FirstName { get; set; } = string.Empty;
        // The user's last name.
        public string LastName { get; set; } = string.Empty;
        // The URL to the user's profile picture.
        public string? AvatarUrl { get; set; }
        // The unique ID from the provider (Subject ID).
        public string ProviderUserId { get; set; } = string.Empty;
    }
}