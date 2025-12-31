using Elastic.Clients.Elasticsearch;
// using Elastic.Clients.Elasticsearch.QueryDsl; // Không cần dòng này nữa
using TicketBooking.Application.Common.Interfaces;
using TicketBooking.Application.Common.Interfaces.AI;
using TicketBooking.Application.Features.Events.Queries.GetEventsList;
using TicketBooking.Infrastructure.Search.Models;

namespace TicketBooking.Infrastructure.Search
{
    public class ElasticSearchService : ISearchService
    {
        private readonly ElasticsearchClient _client;
        private readonly IAiEmbeddingService _aiService;

        public ElasticSearchService(ElasticsearchClient client, IAiEmbeddingService aiService)
        {
            _client = client;
            _aiService = aiService;
        }

        public async Task<List<EventListDto>> SearchAsync(string keyword, CancellationToken cancellationToken)
        {
            // 1. Chuyển từ khóa thành Vector
            var queryVector = await _aiService.GenerateEmbeddingAsync(keyword);

            // 2. Tìm kiếm KNN
            var response = await _client.SearchAsync<EventDocument>(s => s
                .Index("events")
                .Size(10) // Giới hạn kết quả trả về
                          // ✅ FIX LỖI CS1503:
                          // 1. Dùng 'KnnSearch' thay vì 'KnnQuery'.
                          // 2. Bọc trong mảng 'new [] { ... }' vì hàm yêu cầu ICollection.
                .Knn(new[]
                {
                    new KnnSearch
                    {
                        Field = "embedding",
                        QueryVector = queryVector,                        
                        NumCandidates = 100  // Số lượng ứng viên
                    }
                }), cancellationToken);

            if (!response.IsValidResponse)
            {
                return new List<EventListDto>();
            }

            // 3. Map kết quả
            return response.Documents.Select(d => new EventListDto(
                Id: d.Id,
                Name: d.Name,
                ShortDescription: d.Description.Length > 100 ? d.Description.Substring(0, 100) + "..." : d.Description,
                StartDateTime: d.StartDate,
                VenueName: d.VenueName,
                VenueAddress: "N/A",
                MinPrice: d.MinPrice,
                CoverImageUrl: d.ImageUrl
            )).ToList();
        }
    }
}