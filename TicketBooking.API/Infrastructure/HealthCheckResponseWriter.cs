using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Text.Json;

namespace TicketBooking.API.Infrastructure
{
    // Class này giúp format kết quả Health Check thành JSON dễ đọc thay vì chỉ trả về text "Healthy".
    public static class HealthCheckResponseWriter
    {
        public static Task WriteResponse(HttpContext context, HealthReport report)
        {
            context.Response.ContentType = "application/json";

            var response = new
            {
                status = report.Status.ToString(),
                checkedAt = DateTime.UtcNow,
                duration = report.TotalDuration,
                // Liệt kê chi tiết từng thành phần (SQL, Redis...)
                details = report.Entries.Select(e => new
                {
                    key = e.Key,
                    status = e.Value.Status.ToString(),
                    description = e.Value.Description,
                    data = e.Value.Data
                })
            };

            return context.Response.WriteAsync(JsonSerializer.Serialize(response));
        }
    }
}