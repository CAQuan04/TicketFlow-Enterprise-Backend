using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using TicketBooking.API.Infrastructure; // Import namespace chứa GlobalExceptionHandler
using TicketBooking.Application;
using TicketBooking.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// --- 1. REGISTER SERVICES (Dependency Injection) ---

// Add Application & Infrastructure
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// Add Controllers
builder.Services.AddControllers();

// Add Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "TicketBooking API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme.",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement()
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" },
                Scheme = "oauth2",
                Name = "Bearer",
                In = ParameterLocation.Header,
            },
            new List<string>()
        }
    });
});

// --- QUAN TRỌNG: CẤU HÌNH XỬ LÝ LỖI GLOBAL ---
builder.Services.AddProblemDetails(); // Thêm service ProblemDetails chuẩn của .NET
builder.Services.AddExceptionHandler<GlobalExceptionHandler>(); // Đăng ký Handler tùy chỉnh của chúng ta

// Cấu hình Authentication (Giữ nguyên như cũ)
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["Secret"];
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey!))
    };
});

// 1. THÊM DỊCH VỤ CORS (Cho phép mọi nguồn truy cập - Dùng cho Dev)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        builder => builder
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader());
});

var app = builder.Build();

// --- 2. MIDDLEWARE PIPELINE (Thứ tự cực kỳ quan trọng) ---

// ⚠️ QUAN TRỌNG NHẤT: UseExceptionHandler phải nằm ĐẦU TIÊN
// Để nó có thể bắt lỗi từ tất cả các middleware bên dưới nó.
app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();


// 2. KÍCH HOẠT CORS (Phải đặt TRƯỚC UseAuthentication và UseAuthorization)
app.UseCors("AllowAll");

// AuthN trước AuthZ
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();


// Create a scope to resolve scoped services like DbContext and DataSeeder.
// Create a scope to resolve scoped services.
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    try
    {
        // 1. Get the DbContext to access Database functions.
        var context = services.GetRequiredService<TicketBooking.Infrastructure.Data.ApplicationDbContext>();

        // 2. AUTOMATICALLY APPLY MIGRATIONS
        // This command checks if there are any migrations in the code that haven't been applied to the DB.
        // If the DB doesn't exist (like your fresh Docker case), it creates the DB and all Tables.
        // Equivalent to running "Update-Database" manually.
        await context.Database.MigrateAsync();

        // 3. SEED DATA
        // Now that the DB and Tables exist, we can safely insert seed data.
        var seeder = services.GetRequiredService<TicketBooking.Infrastructure.Data.DataSeeder>();
        await seeder.SeedAsync();
    }
    catch (Exception ex)
    {
        // Log any errors during startup (crucial for debugging Docker issues).
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while initializing the database.");
    }
}

app.Run();