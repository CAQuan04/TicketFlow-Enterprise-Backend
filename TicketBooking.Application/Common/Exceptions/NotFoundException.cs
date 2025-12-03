namespace TicketBooking.Application.Common.Exceptions
{
    // Custom exception to be thrown when a requested resource (ID, Record) is not found.
    public class NotFoundException : Exception
    {
        // Constructor accepting the name of the entity and the key that was not found.
        public NotFoundException(string name, object key)
            : base($"Entity \"{name}\" ({key}) was not found.") // Format a standard error message.
        {
        }
    }
}