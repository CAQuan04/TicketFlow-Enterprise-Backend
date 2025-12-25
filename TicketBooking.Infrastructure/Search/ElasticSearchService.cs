using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.QueryDsl;
using TicketBooking.Application.Common.Interfaces;
using TicketBooking.Application.Features.Events.Queries.GetEventsList;
using TicketBooking.Infrastructure.Search.Models;

namespace TicketBooking.Infrastructure.Search
{
    public class ElasticSearchService : ISearchService
    {
        private readonly ElasticsearchClient _client;

        public ElasticSearchService(ElasticsearchClient client)
        {
            _client = client;
        }

        public async Task<List<EventListDto>> SearchAsync(string keyword, CancellationToken cancellationToken)
        {
            // FUZZY SEARCH QUERY
            var response = await _client.SearchAsync<EventDocument>(s => s
                .Indices("events") // ✅ FIX 1: Đổi .Index thành .Indices
                .Query(q => q
                    .MultiMatch(m => m
                        // ✅ FIX 2: Dùng mảng chuỗi để định nghĩa Field và Boost (^3)
                        // Cách này gọn hơn và tránh lỗi Lambda Expression của thư viện mới
                        .Fields(new[] { "name^3", "description", "venueName" })
                        .Query(keyword)
                        .Fuzziness(new Fuzziness("AUTO"))
                    )
                ), cancellationToken);

            if (!response.IsValidResponse)
            {
                return new List<EventListDto>();
            }

            // Map lại sang DTO của Application Layer
            return response.Documents.Select(d => new EventListDto(
               Id: d.Id,
               Name: d.Name,

               // 👇 SỬA DÒNG NÀY: Đổi Description thành ShortDescription
               ShortDescription: d.Description,

               StartDateTime: d.StartDate,
               VenueName: d.VenueName,
               MinPrice: d.MinPrice,
               CoverImageUrl: d.ImageUrl,

               // 👇 Nếu DTO của sếp yêu cầu VenueAddress, hãy giữ dòng này. 
               // Nếu báo lỗi "VenueAddress not found" thì xóa dòng này đi.
               VenueAddress: ""
           )).ToList();
        }
    }
}