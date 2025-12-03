namespace TicketBooking.Domain.Enums
{
    // Enum to define supported social login providers.
    public enum LoginProvider
    {
        // Standard login (Email/Password).
        System = 0,
        // Login via Google.
        Google = 1,
        // Placeholder for future (Facebook).
        Facebook = 2
    }
}