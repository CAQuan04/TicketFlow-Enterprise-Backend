using Elastic.Clients.Elasticsearch;
using MediatR;
using Microsoft.Extensions.Logging;
using TicketBooking.Application.Common.Interfaces.AI; // Dùng Interface vừa tạo
using TicketBooking.Domain.Events;
using TicketBooking.Infrastructure.Search.Models;

namespace TicketBooking.Infrastructure.Search
{
    public class SyncEventToElasticHandler : INotificationHandler<EventPublishedEvent>
    {
        private readonly ElasticsearchClient _elasticClient;
        private readonly IAiEmbeddingService _aiService; // Inject AI
        private readonly ILogger<SyncEventToElasticHandler> _logger;

        public SyncEventToElasticHandler(
            ElasticsearchClient elasticClient,
            IAiEmbeddingService aiService,
            ILogger<SyncEventToElasticHandler> logger)
        {
            _elasticClient = elasticClient;
            _aiService = aiService;
            _logger = logger;
        }

        public async Task Handle(EventPublishedEvent notification, CancellationToken cancellationToken)
        {
            var evt = notification.Event;

            // 1. Tạo đoạn văn bản tổng hợp để AI hiểu
            // Ví dụ: "Đại nhạc hội BlackPink tại Sân Mỹ Đình vào ngày..."
            var textToEmbed = $"{evt.Name}. {evt.Description}. Location: {evt.Venue?.Name}";

            // 2. Gọi Google Gemini để lấy Vector (Embedding)
            var vector = await _aiService.GenerateEmbeddingAsync(textToEmbed);

            // 3. Map sang Document
            var doc = new EventDocument
            {
                Id = evt.Id,
                Name = evt.Name,
                Description = evt.Description,
                VenueName = evt.Venue?.Name ?? "",
                StartDate = evt.StartDateTime,
                ImageUrl = evt.CoverImageUrl,
                MinPrice = 0, // Logic giá tính sau
                Embedding = vector // Lưu vector vào Elastic
            };

            // 4. Index vào Elastic
            await _elasticClient.IndexAsync(doc, idx => idx.Index("events"), cancellationToken);

            _logger.LogInformation($"Synced Event '{evt.Name}' with AI Vector to Elasticsearch.");
        }
    }
}