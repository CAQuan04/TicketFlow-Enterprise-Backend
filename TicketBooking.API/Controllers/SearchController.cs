using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TicketBooking.API.Controllers;
using TicketBooking.Application.Common.Interfaces;

namespace TicketBooking.API.Controllers
{
    public class SearchController : ApiControllerBase
    {
        private readonly ISearchService _searchService;

        public SearchController(ISearchService searchService)
        {
            _searchService = searchService;
        }

        // Endpoint: GET api/search/smart?keyword=blackpink
        [HttpGet("smart")]
        [AllowAnonymous] // Public cho mọi người dùng
        public async Task<IActionResult> SmartSearch([FromQuery] string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword))
            {
                return BadRequest("Keyword is required.");
            }

            var results = await _searchService.SearchAsync(keyword, CancellationToken.None);
            return Ok(results);
        }
    }
}