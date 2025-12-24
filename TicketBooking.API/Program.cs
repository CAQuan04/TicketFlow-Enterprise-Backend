using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using TicketBooking.API.Infrastructure;
using TicketBooking.Application;
using TicketBooking.Infrastructure;
using TicketBooking.Infrastructure.Hubs;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;
using TicketBooking.Infrastructure.Data;

var builder = WebApplication.CreateBuilder(args);

// ==================================================================
// 1. SERVICE REGISTRATION (DEPENDENCY INJECTION)
// ==================================================================

// A. Core Services
builder.Services.AddControllers();
builder.Services.AddSignalR();
builder.Services.AddEndpointsApiExplorer();

// B. Layer Services (Clean Architecture)
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// C. Security & Authentication
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

    // WebSocket Auth Support (SignalR)
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var accessToken = context.Request.Query["access_token"];
            var path = context.HttpContext.Request.Path;
            if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs/notifications"))
            {
                context.Token = accessToken;
            }
            return Task.CompletedTask;
        }
    };
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
        builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});

// D. Error Handling
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

// E. Health Checks
builder.Services.AddHealthChecks()
    .AddDbContextCheck<ApplicationDbContext>(name: "Database")
    .AddRedis(builder.Configuration.GetConnectionString("Redis")!, name: "Redis Cache");

// F. Rate Limiting
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddFixedWindowLimiter("GlobalLimiter", limiterOptions =>
    {
        limiterOptions.PermitLimit = 100;
        limiterOptions.Window = TimeSpan.FromMinutes(1);
        limiterOptions.QueueLimit = 2;
        limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
    });
});

// G. Resilience (Polly)
builder.Services.AddHttpClient("ExternalApi")
    .AddStandardResilienceHandler();

// H. Background Workers
builder.Services.AddHostedService<TicketBooking.Infrastructure.BackgroundJobs.ExpiredOrderCleanupWorker>();

// I. Swagger
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
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
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

var app = builder.Build();

// ==================================================================
// 2. MIDDLEWARE PIPELINE (REQUEST PROCESSING)
// ==================================================================

app.UseExceptionHandler(); // Luôn đặt đầu tiên để bắt mọi lỗi.

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

// CORS -> AuthN -> RateLimit -> AuthZ
app.UseCors("AllowAll");
app.UseAuthentication();
app.UseRateLimiter(); // Đặt sau Auth để sẵn sàng limit theo User ID nếu cần sau này.
app.UseAuthorization();

// ==================================================================
// 3. ENDPOINT MAPPING
// ==================================================================

app.MapHub<NotificationHub>("/hubs/notifications");

app.MapHealthChecks("/health", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    ResponseWriter = HealthCheckResponseWriter.WriteResponse
}).AllowAnonymous();

app.MapControllers().RequireRateLimiting("GlobalLimiter");

// ==================================================================
// 4. DATA SEEDING (AUTO MIGRATION)
// ==================================================================

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<TicketBooking.Infrastructure.Data.ApplicationDbContext>();
        await context.Database.MigrateAsync();

        var seeder = services.GetRequiredService<TicketBooking.Infrastructure.Data.DataSeeder>();
        await seeder.SeedAsync();
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while initializing the database.");
    }
}

app.Run();