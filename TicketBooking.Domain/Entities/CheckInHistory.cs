using TicketBooking.Domain.Common;

namespace TicketBooking.Domain.Entities
{
    // Bảng lưu lịch sử soát vé. 
    // Dùng để tra soát nếu có khiếu nại (VD: Khách bảo chưa vào mà hệ thống báo đã vào).
    public class CheckInHistory : BaseEntity
    {
        public Guid TicketId { get; set; }
        public Ticket Ticket { get; set; } = null!;

        // ID của nhân viên soát vé (Người cầm máy quét).
        public string InspectorId { get; set; } = string.Empty;

        // Thời gian quét.
        public DateTime CheckInTime { get; set; } = DateTime.UtcNow;

        // Kết quả quét (Thành công hay Thất bại).
        public bool IsSuccess { get; set; }

        // Ghi chú (Ví dụ: "Vé giả", "Sai ngày", "Thành công").
        public string Note { get; set; } = string.Empty;
    }
}