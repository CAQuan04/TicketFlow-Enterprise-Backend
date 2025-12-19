namespace TicketBooking.Application.Features.Organizers.Queries.GetEventStats
{
    // Class chứa dữ liệu tổng quan cho biểu đồ và báo cáo của Organizer.
    public class OrganizerEventStatsDto
    {
        // Tổng doanh thu của sự kiện này (Tổng tiền từ các vé đã bán).
        public decimal TotalRevenue { get; set; }

        // Tổng số vé đã bán ra thành công.
        public int TotalTicketsSold { get; set; }

        // Danh sách chi tiết: Loại vé nào bán chạy nhất (Ví dụ: VIP bán 10, Regular bán 50).
        public List<TicketTypeStatDto> TicketTypeBreakdown { get; set; } = new();

        // Danh sách dữ liệu vẽ biểu đồ đường (Line Chart): Doanh thu theo ngày.
        public List<DailyRevenueDto> ChartData { get; set; } = new();
    }

    // DTO con: Thống kê theo loại vé.
    public class TicketTypeStatDto
    {
        // Tên loại vé (VIP, GA...).
        public string TicketTypeName { get; set; } = string.Empty;
        // Số lượng đã bán.
        public int SoldCount { get; set; }
        // Doanh thu từ loại vé này.
        public decimal Revenue { get; set; }
    }

    // DTO con: Dữ liệu biểu đồ.
    public class DailyRevenueDto
    {
        // Ngày bán (Trục hoành X).
        public DateTime Date { get; set; }
        // Doanh thu trong ngày đó (Trục tung Y).
        public decimal DailyRevenue { get; set; }
    }
}