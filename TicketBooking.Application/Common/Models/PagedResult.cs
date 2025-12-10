namespace TicketBooking.Application.Common.Models
{
    // Class Generic dùng chung cho toàn bộ hệ thống khi cần trả về danh sách phân trang.
    public class PagedResult<T>
    {
        public List<T> Items { get; set; } // Dữ liệu của trang hiện tại.
        public int PageIndex { get; set; } // Trang hiện tại (1, 2, 3...).
        public int TotalPages { get; set; } // Tổng số trang tính được.
        public int TotalCount { get; set; } // Tổng số bản ghi thỏa mãn điều kiện tìm kiếm.
        public bool HasPreviousPage => PageIndex > 1; // Helper cho Frontend: Có nút "Trang trước" không?
        public bool HasNextPage => PageIndex < TotalPages; // Helper cho Frontend: Có nút "Trang sau" không?

        public PagedResult(List<T> items, int count, int pageIndex, int pageSize)
        {
            Items = items;
            TotalCount = count;
            PageIndex = pageIndex;
            // Công thức tính tổng số trang: Làm tròn lên (Ceiling).
            // Ví dụ: 101 records, size 10 => 10.1 => 11 trang.
            TotalPages = (int)Math.Ceiling(count / (double)pageSize);
        }
    }
}   