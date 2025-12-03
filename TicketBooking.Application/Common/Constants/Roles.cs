namespace TicketBooking.Domain.Constants
{
    // Static class to hold constant role names, avoiding "magic strings" in the codebase.
    public abstract class Roles
    {
        // Role for System Administrators with full access.
        public const string Admin = nameof(Admin);

        // Role for Event Organizers who create events.
        public const string Organizer = nameof(Organizer);

        // Role for regular End Users who buy tickets.
        public const string Customer = nameof(Customer);

        // Role for Staff who check tickets at the gate.
        public const string TicketInspector = nameof(TicketInspector);

        // Role for Staff assisting the Organizer.
        public const string EventManager = nameof(EventManager);

        // Role for Accountants who can only view financial data.
        public const string FinanceViewer = nameof(FinanceViewer);
    }
}