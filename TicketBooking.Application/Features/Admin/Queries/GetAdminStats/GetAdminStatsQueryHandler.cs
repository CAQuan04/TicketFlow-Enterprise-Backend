using Dapper; // Import Dapper để dùng Extension Method (.QueryFirstAsync).
using MediatR; // Import MediatR.
using TicketBooking.Application.Common.Interfaces.Data; // Import Connection Factory.
using TicketBooking.Domain.Enums; // Import Enum để so sánh trạng thái.

namespace TicketBooking.Application.Features.Admin.Queries.GetAdminStats
{
    // Handler xử lý logic thống kê bằng SQL thuần.
    public class GetAdminStatsQueryHandler : IRequestHandler<GetAdminStatsQuery, AdminStatsDto>
    {
        // Factory để tạo kết nối SQL.
        private readonly ISqlConnectionFactory _sqlConnectionFactory;

        // Constructor Injection.
        public GetAdminStatsQueryHandler(ISqlConnectionFactory sqlConnectionFactory)
        {
            _sqlConnectionFactory = sqlConnectionFactory; // Gán factory.
        }

        // Hàm xử lý chính.
        public async Task<AdminStatsDto> Handle(GetAdminStatsQuery request, CancellationToken cancellationToken)
        {
            using var connection = _sqlConnectionFactory.CreateConnection();

            // SQL CHUẨN (Đã bọc ngoặc vuông và dùng số)
            // Sếp hãy đảm bảo tên bảng trong DB khớp với tên trong ngoặc vuông này (Users, Orders...)
            // 2. Viết câu lệnh SQL thuần (Raw SQL).
            // PHIÊN BẢN FINAL: Khớp 100% với dữ liệu thực tế trong ảnh Sếp gửi.
            const string sql = @"
                SELECT
                    -- 1. Doanh thu: Orders so sánh với CHỮ ('Paid')
                    (SELECT COALESCE(SUM(TotalAmount), 0) FROM [Orders] WHERE Status = 'Paid') AS TotalRevenue,

                    -- 2. Số vé bán ra: Đếm bảng Tickets
                    (SELECT COUNT(*) FROM [Tickets]) AS TotalTicketsSold,

                    -- 3. Sự kiện: Events so sánh với SỐ (1 = Published)
                    (SELECT COUNT(*) FROM [Events] WHERE Status = 1) AS TotalEvents,

                    -- 4. User: Users so sánh với CHỮ ('Customer')
                    (SELECT COUNT(*) FROM [Users] WHERE Role = 'Customer') AS TotalUsers
            ";

            try
            {
                var stats = await connection.QueryFirstOrDefaultAsync<AdminStatsDto>(sql);
                return stats ?? new AdminStatsDto(0, 0, 0, 0);
            }
            catch (Exception ex)
            {
                // Nếu lỗi, đoạn này sẽ giúp Sếp nhìn thấy lỗi ngay tại chỗ breakpoint
                Console.WriteLine("DAPPER ERROR: " + ex.Message);
                throw; // Ném ra để GlobalHandler bắt
            }
        }
    }
}