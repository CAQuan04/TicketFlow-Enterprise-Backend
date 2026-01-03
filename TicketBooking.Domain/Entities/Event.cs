using TicketBooking.Domain.Common;
using TicketBooking.Domain.Enums;

namespace TicketBooking.Domain.Entities
{
    public class Event : BaseEntity
    {
        // 1. Giới hạn số vé tối đa một người được mua cho sự kiện này.
        // Mặc định là 5 vé (để chống phe vé).
        public int MaxTicketsPerUser { get; set; } = 5;

        // 2. Thời điểm mở bán vé (Countdown).
        // Trước giờ này, nút mua sẽ bị khóa.
        public DateTime TicketSaleStartTime { get; set; }

        // 3. Thời điểm đóng bán vé (Hết hạn).
        // Sau giờ này, không mua được nữa (dù còn vé).
        public DateTime? TicketSaleEndTime { get; set; }

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