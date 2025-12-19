using Dapper; // Sử dụng Dapper cho hiệu năng cao.
using MediatR; // Sử dụng Pattern CQRS.
using TicketBooking.Application.Common.Exceptions; // Sử dụng NotFoundException/UnauthorizedException.
using TicketBooking.Application.Common.Interfaces; // Sử dụng ICurrentUserService.
using TicketBooking.Application.Common.Interfaces.Data; // Sử dụng ISqlConnectionFactory.
using TicketBooking.Domain.Entities; // Sử dụng Entity Event để lấy tên bảng.

namespace TicketBooking.Application.Features.Organizers.Queries.GetEventStats
{
    // Handler xử lý logic thống kê chi tiết cho Organizer.
    public class GetEventStatsQueryHandler : IRequestHandler<GetEventStatsQuery, OrganizerEventStatsDto>
    {
        // Factory để tạo kết nối SQL (Raw SQL).
        private readonly ISqlConnectionFactory _sqlConnectionFactory;
        // Service để lấy ID của người dùng đang đăng nhập (từ Token JWT).
        private readonly ICurrentUserService _currentUserService;

        // Constructor Injection.
        public GetEventStatsQueryHandler(
            ISqlConnectionFactory sqlConnectionFactory,
            ICurrentUserService currentUserService)
        {
            _sqlConnectionFactory = sqlConnectionFactory; // Gán factory kết nối.
            _currentUserService = currentUserService; // Gán service lấy user hiện tại.
        }

        // Phương thức xử lý chính.
        public async Task<OrganizerEventStatsDto> Handle(GetEventStatsQuery request, CancellationToken cancellationToken)
        {
            // 1. Lấy ID của User hiện tại (Organizer đang gọi API).
            var currentUserId = _currentUserService.UserId;

            // 2. Mở kết nối SQL.
            using var connection = _sqlConnectionFactory.CreateConnection();

            // 3. SECURITY CHECK (DATA ISOLATION).
            // Kiểm tra xem EventId này có thực sự thuộc về User đang đăng nhập không.
            // Nếu không phải chính chủ -> Chặn ngay lập tức.
            // Sử dụng SQL thuần để check nhanh.
            const string ownerCheckSql = "SELECT CreatedBy FROM Events WHERE Id = @EventId";

            // Thực thi query lấy người tạo Event.
            var ownerId = await connection.ExecuteScalarAsync<string>(ownerCheckSql, new { request.EventId });

            // Nếu không tìm thấy Event -> Lỗi 404.
            if (ownerId == null)
            {
                throw new NotFoundException(nameof(Event), request.EventId);
            }

            // Nếu người gọi API không phải người tạo (Owner) -> Lỗi 403 Forbidden (hoặc Unauthorized).
            // So sánh String (Guid) không phân biệt hoa thường.
            if (!string.Equals(ownerId, currentUserId, StringComparison.OrdinalIgnoreCase))
            {
                // Ném lỗi truy cập trái phép. Hacker hoặc Organizer khác đang cố xem dữ liệu.
                throw new UnauthorizedAccessException("You are not the owner of this event.");
            }

            // 4. PREPARE ANALYTICS SQL (DAPPER MULTI-MAPPING).
            // Chúng ta cần join 3 bảng: Orders -> Tickets -> TicketTypes để tính toán.
            // Lưu ý: Chỉ tính các đơn hàng đã thanh toán (Orders.Status = 1 - Paid).

            const string statsSql = @"
    -- QUERY 1: TỔNG QUAN (Sửa Status = 'Paid')
    SELECT 
        COALESCE(SUM(tt.Price), 0) AS TotalRevenue,
        COUNT(t.Id) AS TotalTicketsSold
    FROM Tickets t
    JOIN TicketTypes tt ON t.TicketTypeId = tt.Id
    JOIN Orders o ON t.OrderId = o.Id
    WHERE tt.EventId = @EventId AND o.Status = 'Paid'; -- SỬA THÀNH CHỮ 'Paid'

    -- QUERY 2: PHÂN TÍCH THEO LOẠI VÉ (Sửa Status = 'Paid')
    SELECT 
        tt.Name AS TicketTypeName,
        COUNT(t.Id) AS SoldCount,
        SUM(tt.Price) AS Revenue
    FROM Tickets t
    JOIN TicketTypes tt ON t.TicketTypeId = tt.Id
    JOIN Orders o ON t.OrderId = o.Id
    WHERE tt.EventId = @EventId AND o.Status = 'Paid' -- SỬA THÀNH CHỮ 'Paid'
    GROUP BY tt.Name;

    -- QUERY 3: BIỂU ĐỒ (Sửa Status = 'Paid')
    SELECT 
        CAST(o.CreatedDate AS DATE) AS [Date],
        SUM(tt.Price) AS DailyRevenue
    FROM Tickets t
    JOIN TicketTypes tt ON t.TicketTypeId = tt.Id
    JOIN Orders o ON t.OrderId = o.Id
    WHERE tt.EventId = @EventId AND o.Status = 'Paid' -- SỬA THÀNH CHỮ 'Paid'
    GROUP BY CAST(o.CreatedDate AS DATE)
    ORDER BY [Date];
";

            // 5. EXECUTE MULTIPLE QUERY.
            // Dapper thực thi tất cả trong 1 round-trip tới Server.
            using var multi = await connection.QueryMultipleAsync(statsSql, new { request.EventId });

            // 6. READ RESULT SETS.
            // Đọc kết quả Query 1 (Tổng quan).
            var overview = await multi.ReadFirstAsync<OrganizerEventStatsDto>();

            // Đọc kết quả Query 2 (Breakdown).
            var breakdown = (await multi.ReadAsync<TicketTypeStatDto>()).ToList();

            // Đọc kết quả Query 3 (Chart Data).
            var chartData = (await multi.ReadAsync<DailyRevenueDto>()).ToList();

            // 7. ASSEMBLE DTO.
            // Gán các list con vào object cha.
            overview.TicketTypeBreakdown = breakdown;
            overview.ChartData = chartData;

            // 8. RETURN RESULT.
            return overview;
        }
    }
}