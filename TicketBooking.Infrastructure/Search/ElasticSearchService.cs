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

            // 2. Tìm kiếm HYBRID (Kết hợp Vector + Từ khóa)
            var response = await _client.SearchAsync<EventDocument>(s => s
                .Index("events")
                .Size(10)

                // PHẦN 1: TÌM THEO VECTOR (SEMANTIC SEARCH) - Hiểu ý
                .Knn(new[]
                {
                    new KnnSearch
                    {
                        Field = "embedding",
                        QueryVector = queryVector,
                        NumCandidates = 100,
                        Boost = 0.5f // Trọng số thấp hơn (0.5): Để AI chỉ đóng vai trò hỗ trợ, gợi ý
                    }
                })

                // PHẦN 2: TÌM THEO TỪ KHÓA (KEYWORD SEARCH) - Chính xác
                .Query(q => q
                    .MultiMatch(m => m
                        .Fields(new[] { "name^3", "description", "venueName" })
                        .Query(keyword)
                        .Boost(1.5f) // Trọng số cao hơn (1.5): Ưu tiên kết quả khớp chữ viết
                    )
                ), cancellationToken);

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