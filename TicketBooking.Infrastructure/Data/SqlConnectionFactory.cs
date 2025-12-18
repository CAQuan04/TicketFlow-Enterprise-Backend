using System.Data; // Dùng cho IDbConnection.
using Microsoft.Data.SqlClient; // Driver SQL Server chuẩn.
using Microsoft.Extensions.Configuration; // Dùng để đọc appsettings.json.
using TicketBooking.Application.Common.Interfaces.Data; // Implement Interface.

namespace TicketBooking.Infrastructure.Data
{
    // Class chịu trách nhiệm sản xuất kết nối SQL.
    public class SqlConnectionFactory : ISqlConnectionFactory
    {
        // Biến lưu trữ chuỗi kết nối (Connection String).
        private readonly string _connectionString;

        // Constructor nhận IConfiguration để đọc cấu hình.
        public SqlConnectionFactory(IConfiguration configuration)
        {
            // Lấy chuỗi kết nối có tên "DefaultConnection" từ file cấu hình.
            // Nếu không tìm thấy thì gán chuỗi rỗng (nhưng thường sẽ throw error lúc chạy).
            _connectionString = configuration.GetConnectionString("DefaultConnection") ?? string.Empty;
        }

        // Thực thi phương thức tạo kết nối.
        public IDbConnection CreateConnection()
        {
            // Tạo mới một đối tượng SqlConnection bằng chuỗi kết nối đã lưu.
            // Lưu ý: Chưa mở kết nối (Open) ngay tại đây, việc mở sẽ do Dapper hoặc người dùng quản lý.
            return new SqlConnection(_connectionString);
        }
    }
}