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
        [HttpGet("/api/events/recommendations")]
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

            // Sử dụng Named Arguments như bài trước để tránh lỗi DTO
            var sortedEvents = recommendedIds
                .Join(events, id => id, e => e.Id, (id, e) => e)
                .Select(e => new EventListDto(
                    Id: e.Id,
                    Name: e.Name,
                    ShortDescription: e.Description.Length > 100 ? e.Description.Substring(0, 100) + "..." : e.Description,
                    StartDateTime: e.StartDateTime,
                    CoverImageUrl: e.CoverImageUrl,
                    VenueName: e.Venue.Name,
                    VenueAddress: e.Venue.Address,
                    MinPrice: e.TicketTypes.Any() ? e.TicketTypes.Min(t => t.Price) : 0
                ))
                .ToList();


            return Ok(sortedEvents);
        }
    }
}