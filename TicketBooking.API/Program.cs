using TicketBooking.Application; // Import namespace
using Microsoft.EntityFrameworkCore;
using TicketBooking.Application.Common.Interfaces;
using TicketBooking.Infrastructure.Data;

namespace TicketBooking.API;

public class Program
{
    public static void Main(string[] args)
    {




        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

        // 2. Đăng ký DbContext với DI Container
        builder.Services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(connectionString));


        builder.Services.AddControllers();
        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();
        // 1. Đăng ký Application Layer (MediatR)
        builder.Services.AddApplication();

        // 2. Đăng ký DbContext (Đã làm ở bước trước)
        // QUAN TRỌNG: Đăng ký Interface ánh xạ vào Class thật
        builder.Services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<ApplicationDbContext>());




        var app = builder.Build();
        

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();

        app.UseAuthorization();


        app.MapControllers();

        app.Run();
    }
}
