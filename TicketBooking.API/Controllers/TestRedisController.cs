using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed; // Thư viện Redis
using System.Text;

namespace TicketBooking.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestRedisController : ControllerBase
    {
        private readonly IDistributedCache _cache;

        public TestRedisController(IDistributedCache cache)
        {
            _cache = cache;
        }

        [HttpGet]
        public async Task<IActionResult> TestConnection()
        {
            try
            {
                // 1. Ghi thử vào Redis
                string key = "Test_Connection";
                string value = $"Hello Redis! Time: {DateTime.UtcNow}";

                var options = new DistributedCacheEntryOptions()
                    .SetAbsoluteExpiration(TimeSpan.FromMinutes(5)); // Cache sống 5 phút

                await _cache.SetStringAsync(key, value, options);

                // 2. Đọc lại từ Redis
                var cachedValue = await _cache.GetStringAsync(key);

                if (cachedValue == value)
                {
                    return Ok(new { Status = "Success", Message = "Redis is working!", Data = cachedValue });
                }
                else
                {
                    return StatusCode(500, "Saved to Redis but read back wrong value.");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Status = "Failed", Error = ex.Message });
            }
        }
    }
}