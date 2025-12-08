using TicketBooking.Domain.Common;
using TicketBooking.Domain.Enums;

namespace TicketBooking.Domain.Entities
{
    public class Event : BaseEntity
    {
        public Guid VenueId { get; set; }
        public Venue Venue { get; set; } = null!;

        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;

        // Thời gian bắt đầu và kết thúc sự kiện.
        public DateTime StartDateTime { get; set; }
        public DateTime EndDateTime { get; set; }

        // Ảnh bìa sự kiện (URL lấy từ File Storage).
        public string? CoverImageUrl { get; set; }

        // Trạng thái sự kiện. Mặc định khi tạo mới sẽ là Draft.
        public EventStatus Status { get; set; } = EventStatus.Draft;

        // Quan hệ 1-N: Một sự kiện có nhiều loại vé (VIP, GA, Early Bird...).
        public ICollection<TicketType> TicketTypes { get; set; } = new List<TicketType>();
    }
}