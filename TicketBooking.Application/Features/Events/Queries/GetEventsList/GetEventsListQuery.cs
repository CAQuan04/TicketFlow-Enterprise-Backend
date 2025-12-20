using MediatR;
using TicketBooking.Application.Common.Models;

namespace TicketBooking.Application.Features.Events.Queries.GetEventsList
{
    // Request Object: Chứa tất cả tham số tìm kiếm.
    public record GetEventsListQuery : IRequest<PagedResult<EventListDto>>
    {
        public int PageIndex { get; init; } = 1; // Mặc định trang 1.
        public int PageSize { get; init; } = 10; // Mặc định 10 item.

        // --- Filters (Nullable) ---
        public string? SearchTerm { get; init; } // Tìm theo tên hoặc mô tả.
        public Guid? VenueId { get; init; }      // Lọc theo địa điểm.
        public DateTime? FromDate { get; init; } // Lọc sự kiện từ ngày này trở đi.
        public DateTime? ToDate { get; init; }   // Đến ngày.
    }
}