using Microsoft.EntityFrameworkCore;
using TicketBooking.Application; // Import namespace
using TicketBooking.Application.Common.Interfaces;
using TicketBooking.Infrastructure;
using TicketBooking.Infrastructure.Data;

namespace TicketBooking.API;

public class Program
{
    public static void Main(string[] args)
    {




        // Tạo Builder để bắt đầu xây dựng ứng dụng Web.
        var builder = WebApplication.CreateBuilder(args);

        // --- 1. ĐĂNG KÝ CÁC SERVICES (DEPENDENCY INJECTION) ---

        // Đăng ký tất cả logic của Application Layer (MediatR, v.v...).
        // Hàm này nằm trong TicketBooking.Application/DependencyInjection.cs
        builder.Services.AddApplication();

        // Đăng ký tất cả hạ tầng của Infrastructure Layer (Db, Auth, JWT...).
        // Hàm này nằm trong TicketBooking.Infrastructure/DependencyInjection.cs
        // Chúng ta truyền 'builder.Configuration' vào để bên dưới đọc được ConnectionString và JwtSettings.
        builder.Services.AddInfrastructure(builder.Configuration);

        // Đăng ký các Controller để xử lý API Request.
        builder.Services.AddControllers();

        // Đăng ký Swagger để tạo tài liệu API tự động.
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        // --- 2. XÂY DỰNG APP ---
        var app = builder.Build();

        // --- 3. CẤU HÌNH PIPELINE (MIDDLEWARE) ---

        // Nếu đang chạy ở môi trường Dev (trên máy bạn), thì bật Swagger lên.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger(); // Tạo file JSON mô tả API.
            app.UseSwaggerUI(); // Tạo giao diện web để test API.
        }

        // Chuyển hướng HTTP sang HTTPS.
        app.UseHttpsRedirection();

        // Kích hoạt tính năng xác thực (Authentication) - Ai đang đăng nhập?
        // (Lưu ý: Chúng ta chưa cấu hình chi tiết JWT Bearer ở đây, sẽ làm ở bước sau, nhưng cứ để sẵn).
        app.UseAuthorization();

        // Map các request vào các Controller tương ứng.
        app.MapControllers();

        // Chạy ứng dụng.
        app.Run();
    }
}
