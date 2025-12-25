using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TicketBooking.API.Controllers;
using TicketBooking.Application.Common.Interfaces;
using TicketBooking.Application.Common.Interfaces.AI;
using TicketBooking.Application.Features.Events.Queries.GetEventsList; // Reuse DTO
using TicketBooking.Domain.Enums;

namespace TicketBooking.API.Controllers
{
    [Authorize] // Phải đăng nhập mới biết ai để gợi ý.
    public class RecommendationsController : ApiControllerBase
    {
        private readonly IRecommendationService _recommendationService;
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUserService;

        public RecommendationsController(
            IRecommendationService recommendationService,
            IApplicationDbContext context,
            ICurrentUserService currentUserService)
        {
            _recommendationService = recommendationService;
            _context = context;
            _currentUserService = currentUserService;
        }

        // GET api/events/recommendations
        [HttpGet("events/recommendations")]
        public async Task<IActionResult> GetRecommendations()
        {
            // 1. Lấy ID người dùng hiện tại.
            var userId = Guid.Parse(_currentUserService.UserId!);

            // 2. Lấy danh sách tất cả Event đang mở bán (Candidates).
            // Chỉ lấy ID để nhẹ database.
            var candidateEventIds = await _context.Events
                .AsNoTracking()
                .Where(e => e.Status == EventStatus.Published && e.StartDateTime > DateTime.UtcNow)
                .Select(e => e.Id)
                .ToListAsync();

            if (!candidateEventIds.Any()) return Ok(new List<EventListDto>());

            // 3. Gọi AI để chấm điểm và sắp xếp (Ranking).
            var recommendedIds = _recommendationService.GetRecommendedEvents(userId, candidateEventIds);

            // 4. Fetch chi tiết thông tin các Event đã được AI chọn.
            // Lưu ý: SQL WHERE IN không bảo toàn thứ tự, nên cần sort lại theo list ID.
            var events = await _context.Events
                .AsNoTracking()
                .Include(e => e.Venue)
                .Include(e => e.TicketTypes)
                .Where(e => recommendedIds.Contains(e.Id))
                .ToListAsync();

            // Sắp xếp lại list events theo đúng thứ tự mà AI đã recommend.
            var sortedEvents = recommendedIds
                .Join(events, id => id, e => e.Id, (id, e) => e)
                .Select(e => new EventListDto(
                    // 👇 DÙNG NAMED ARGUMENTS (TênBiến: GiáTrị) ĐỂ SỬA LỖI
                    Id: e.Id,
                    Name: e.Name,

                    // Lưu ý: Kiểm tra xem DTO của sếp là Description hay ShortDescription
                    // Nếu báo đỏ chữ ShortDescription thì đổi thành Description
                    ShortDescription: e.Description.Length > 100 ? e.Description.Substring(0, 100) + "..." : e.Description,

                    StartDateTime: e.StartDateTime,
                    CoverImageUrl: e.CoverImageUrl,
                    VenueName: e.Venue.Name,

                    // 👇 Khả năng cao DTO của sếp có trường này nên mới bị lệch tham số
                    VenueAddress: e.Venue.Address,

                    // 👇 Đây là tham số bị báo lỗi, giờ đã được gán đích danh
                    MinPrice: e.TicketTypes.Any() ? e.TicketTypes.Min(t => t.Price) : 0
                ))
                .ToList();

            return Ok(sortedEvents);
        }
    }
}