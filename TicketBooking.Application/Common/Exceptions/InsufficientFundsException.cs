namespace TicketBooking.Application.Common.Exceptions
{
    public class InsufficientFundsException : Exception
    {
        public InsufficientFundsException(Guid userId)
            : base($"User {userId} does not have enough balance to proceed.") { }
    }
}