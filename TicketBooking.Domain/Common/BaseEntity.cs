namespace TicketBooking.Domain.Common
{
    // Class cha cho tất cả các Entity trong hệ thống.
    public abstract class BaseEntity
    {
        public Guid Id { get; set; } = Guid.NewGuid(); // Khóa chính.

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow; // Ngày tạo.

        // MỚI THÊM: Lưu ID của người tạo ra bản ghi này.
        // Dùng để check quyền sở hữu (Data Ownership).
        public string? CreatedBy { get; set; }

        public DateTime? LastModifiedDate { get; set; } // Ngày sửa cuối cùng.

        // MỚI THÊM (Optional): Lưu ID của người sửa cuối cùng.
        public string? LastModifiedBy { get; set; }
    }
}