namespace TicketBooking.Application.Features.Tickets.Queries.GetMyTickets
{
    public record MyTicketDto(
        Guid Id,
        string TicketCode,      // Mã bí mật để tạo QR.
        string EventName,       // Tên sự kiện.
        string VenueName,       // Tên địa điểm.
        string VenueAddress,    // Địa chỉ.
        DateTime StartDateTime, // Giờ diễn ra.
        string TicketTypeName,  // Loại vé (VIP/Standard).
        decimal Price,          // Giá vé.
        string Status,          // Active/Used/Cancelled.
        Guid OrderId,           // Link về đơn hàng gốc.
        string? CoverImageUrl   // Ảnh bìa sự kiện.
    );
}