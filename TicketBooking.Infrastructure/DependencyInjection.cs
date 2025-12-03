using Microsoft.EntityFrameworkCore; // Thư viện để làm việc với Entity Framework Core.
using Microsoft.Extensions.Configuration; // Thư viện để đọc file appsettings.json.
using Microsoft.Extensions.DependencyInjection; // Thư viện để đăng ký các Service (DI).
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TicketBooking.Application.Common.Interfaces; // Namespace chứa Interface của DbContext.
using TicketBooking.Application.Common.Interfaces.Authentication; // Namespace chứa Interface bảo mật.
using TicketBooking.Infrastructure.Authentication; // Namespace chứa class thực thi bảo mật.
using TicketBooking.Infrastructure.Data;
using TicketBooking.Infrastructure.Services; // Namespace chứa ApplicationDbContext.

namespace TicketBooking.Infrastructure
{
    // Class tĩnh dùng để gom nhóm các cấu hình Dependency Injection cho Infrastructure layer.
    public static class DependencyInjection
    {
        // Hàm mở rộng (Extension method) giúp Program.cs gọi ngắn gọn: .AddInfrastructure(...)
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            // --- 1. CẤU HÌNH DATABASE ---

            // Đăng ký ApplicationDbContext vào DI Container.
            services.AddDbContext<ApplicationDbContext>(options =>
                // Sử dụng SQL Server với Connection String lấy từ cấu hình.
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

            // Đăng ký Interface IApplicationDbContext ánh xạ vào class ApplicationDbContext thực tế.
            // Điều này giúp lớp Application dùng được DB mà không phụ thuộc trực tiếp vào EF Core.
            services.AddScoped<IApplicationDbContext>(provider =>
                provider.GetRequiredService<ApplicationDbContext>());

            // --- 2. CẤU HÌNH BẢO MẬT (AUTH) ---

            // Đọc phần "JwtSettings" từ appsettings.json và map vào class JwtSettings.
            services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.SectionName));

            // Đăng ký dịch vụ băm mật khẩu (BCrypt) dạng Singleton (vì nó không lưu trạng thái).
            services.AddSingleton<IPasswordHasher, BCryptPasswordHasher>();

            // Đăng ký dịch vụ tạo Token (JWT) dạng Singleton.
            services.AddSingleton<IJwtTokenGenerator, JwtTokenGenerator>();

            // Thêm dòng này vào method AddInfrastructure:
            services.AddTransient<DataSeeder>(); // Register DataSeeder as Transient.

            // Add inside AddInfrastructure method:
            services.Configure<EmailSettings>(configuration.GetSection(EmailSettings.SectionName));
            services.AddTransient<IEmailService, SmtpEmailService>();

            // Trả về services để có thể viết code nối tiếp (Fluent API).
            return services;
        }
    }
}
