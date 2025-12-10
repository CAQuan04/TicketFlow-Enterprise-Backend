namespace TicketBooking.Application.Features.Events.Queries.GetEventsList
{
    // Sử dụng record để tối ưu hiệu năng và tính bất biến.
    public record EventListDto(
        Guid Id,
        string Name,
        string ShortDescription, // Chỉ hiển thị mô tả ngắn.
        DateTime StartDateTime,
        string? CoverImageUrl,
        string VenueName,       // Flatten Data: Tên địa điểm (Joined từ bảng Venue).
        string VenueAddress,    // Flatten Data: Địa chỉ.
        decimal MinPrice        // Business Value: Giá vé thấp nhất (để hiển thị "Từ 500k...").
    );
}