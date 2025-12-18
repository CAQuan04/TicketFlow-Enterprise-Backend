namespace TicketBooking.Application.Features.Admin.Queries.GetAdminStats
{
    // Class chứa dữ liệu báo cáo trả về cho Admin.
    public record AdminStatsDto(
        decimal TotalRevenue, // Tổng doanh thu (Tiền).
        int TotalTicketsSold, // Tổng số vé đã bán.
        int TotalEvents,      // Tổng số sự kiện đang chạy.
        int TotalUsers        // Tổng số khách hàng.
    );
}