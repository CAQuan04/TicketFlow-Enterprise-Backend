namespace TicketBooking.Application.Features.Events.Queries.GetEventDetail
{
    // Cấu trúc phẳng (Flat structure) giúp Frontend dễ hiển thị.
    public record EventDetailDto(
        Guid Id,
        string Name,
        string Description,
        DateTime StartDateTime,
        DateTime EndDateTime,
        string? CoverImageUrl,
        string VenueName,       // Flatten data: Lấy tên Venue ra ngoài.
        string VenueAddress,    // Flatten data: Lấy địa chỉ Venue ra ngoài.
        string VenueCity,
        List<TicketTypeDetailDto> TicketTypes
    );
}