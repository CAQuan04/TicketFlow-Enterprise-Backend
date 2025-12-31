using Elastic.Clients.Elasticsearch;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TicketBooking.API.Controllers;
using TicketBooking.Application.Common.Interfaces;
using TicketBooking.Domain.Constants;
using TicketBooking.Domain.Enums;
using TicketBooking.Infrastructure.Search.Models;

namespace TicketBooking.API.Controllers
{
    public class SearchController : ApiControllerBase
    {
        private readonly ISearchService _searchService;
        private readonly ElasticsearchClient _elasticClient; // Inject thêm Client
        private readonly IApplicationDbContext _context; // Inject thêm DB Context

        public SearchController(ISearchService searchService,
            ElasticsearchClient elasticClient,
            IApplicationDbContext context)
        {
            _searchService = searchService; _elasticClient = elasticClient;
            _context = context;
        }

        [HttpGet("smart")]
        public async Task<IActionResult> SmartSearch(string keyword)
        {
            var result = await _searchService.SearchAsync(keyword, CancellationToken.None);
            return Ok(result);
        }

        // Endpoint: POST api/search/sync-all
        // Tác dụng: Lấy tất cả sự kiện đã Published trong SQL và ném vào Elastic.
        [Authorize(Roles = Roles.Admin)]
        [HttpPost("sync-all")]
        public async Task<IActionResult> SyncAllData()
        {
            // 1. Lấy toàn bộ Event đã duyệt từ SQL
            var events = await _context.Events
                .Include(e => e.Venue)
                .Where(e => e.Status == EventStatus.Published)
                .ToListAsync();

            if (!events.Any()) return Ok("No published events found in SQL.");

            var count = 0;
            foreach (var evt in events)
            {
                // 2. Map sang Document
                var doc = new EventDocument
                {
                    Id = evt.Id,
                    Name = evt.Name,
                    Description = evt.Description,
                    VenueName = evt.Venue?.Name ?? "Unknown",
                    StartDate = evt.StartDateTime,
                    ImageUrl = evt.CoverImageUrl,
                    MinPrice = 0 // Tạm thời để 0
                };

                // 3. Đẩy vào Elastic (Upsert)
                var response = await _elasticClient.IndexAsync(doc, idx => idx.Index("events"));
                if (response.IsValidResponse) count++;
            }

            return Ok(new { Message = $"Synced {count}/{events.Count} events to Elasticsearch." });
        }
    }
}