using System.Data; // Cần thiết để dùng IDbConnection.

namespace TicketBooking.Application.Common.Interfaces.Data
{
    // Interface trừu tượng hóa việc tạo kết nối database.
    // Giúp tách biệt logic kết nối khỏi logic truy vấn.
    public interface ISqlConnectionFactory
    {
        // Phương thức tạo ra một kết nối mới tới SQL Server.
        IDbConnection CreateConnection();
    }
}