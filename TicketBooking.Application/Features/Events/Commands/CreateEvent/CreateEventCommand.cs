using MediatR;

namespace TicketBooking.Application.Features.Events.Commands.CreateEvent
{
    // Lệnh yêu cầu tạo sự kiện. Chứa đầy đủ thông tin Header (Event) và Lines (TicketTypes).
    public record CreateEventCommand(
        Guid VenueId,
        string Name,
        string Description,
        DateTime StartDateTime,
        DateTime EndDateTime,
        string? CoverImageUrl,
        List<TicketTypeDto> TicketTypes // Danh sách loại vé đi kèm.
    ) : IRequest<Guid>; // Trả về ID của sự kiện vừa tạo.
}