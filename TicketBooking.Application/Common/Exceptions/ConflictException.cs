namespace TicketBooking.Application.Common.Exceptions
{
    // Exception dùng cho trường hợp xung đột dữ liệu (Concurrency).
    // Sẽ được map thành HTTP 409 Conflict ở GlobalExceptionHandler.
    public class ConflictException : Exception
    {
        public ConflictException(string message) : base(message) { }
    }
}