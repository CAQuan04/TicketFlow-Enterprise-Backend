using Elastic.Clients.Elasticsearch;
using MediatR;
using Microsoft.Extensions.Logging;
using TicketBooking.Domain.Events;
using TicketBooking.Infrastructure.Search.Models;

namespace TicketBooking.Infrastructure.Search
{
    // Handler này lắng nghe sự kiện EventPublishedEvent.
    // Nhiệm vụ: Copy dữ liệu từ SQL Entity sang Elastic Document.
    public class SyncEventToElasticHandler : INotificationHandler<EventPublishedEvent>
    {
        private readonly ElasticsearchClient _elasticClient;
        private readonly ILogger<SyncEventToElasticHandler> _logger;

        public SyncEventToElasticHandler(ElasticsearchClient elasticClient, ILogger<SyncEventToElasticHandler> logger)
        {
            _elasticClient = elasticClient;
            _logger = logger;
        }

        public async Task Handle(EventPublishedEvent notification, CancellationToken cancellationToken)
        {
            var evt = notification.Event;

            // 1. Map Entity to Document.
            var doc = new EventDocument
            {
                Id = evt.Id,
                Name = evt.Name,
                Description = evt.Description,
                VenueName = evt.Venue?.Name ?? "Unknown", // Handle null navigation if not eager loaded (careful here).
                StartDate = evt.StartDateTime,
                ImageUrl = evt.CoverImageUrl,
                // Giả sử logic tính giá min đơn giản, thực tế cần query TicketTypes.
                MinPrice = 0
            };

            // 2. Index into Elasticsearch.
            // Nếu ID đã tồn tại, nó sẽ Update (Upsert). Nếu chưa, nó Create.
            var response = await _elasticClient.IndexAsync(doc, idx => idx.Index("events"), cancellationToken);

            if (response.IsValidResponse)
            {
                _logger.LogInformation($"Successfully synced Event {evt.Id} to Elasticsearch.");
            }
            else
            {
                _logger.LogError($"Failed to sync Event {evt.Id}: {response.DebugInformation}");
            }
        }
    }
}