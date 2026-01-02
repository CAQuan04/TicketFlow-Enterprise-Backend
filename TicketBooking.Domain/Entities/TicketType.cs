using TicketBooking.Domain.Common;

namespace TicketBooking.Domain.Entities
{
    public class TicketType : BaseEntity
    {
        public Guid EventId { get; set; }
        public Event Event { get; set; } = null!;

        public string Name { get; set; } = string.Empty; // Ví dụ: VIP, Regular
        public decimal Price { get; set; }

        // Tổng số lượng vé phát hành ban đầu.
        public int Quantity { get; set; }

        // Số lượng vé còn lại có thể bán.
        // Logic: Khi mới tạo, AvailableQuantity phải bằng Quantity.
        public int AvailableQuantity { get; set; }

       
        public decimal? OriginalPrice { get; set; }
        
        public string? Description { get; set; }

        // --- CONCURRENCY TOKEN ---
        // Đây là trường đặc biệt. Mỗi khi dữ liệu dòng này thay đổi, 
        // SQL Server sẽ tự động thay đổi giá trị binary này.
        public byte[] RowVersion { get; set; } = null!;
    }
}