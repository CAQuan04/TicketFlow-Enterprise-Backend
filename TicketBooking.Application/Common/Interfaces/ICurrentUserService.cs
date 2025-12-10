namespace TicketBooking.Application.Common.Interfaces
{
    // Interface giúp lấy thông tin người dùng hiện tại ở bất kỳ đâu trong hệ thống.
    public interface ICurrentUserService
    {
        string? UserId { get; } // Lấy User ID từ Claims.
    }
}