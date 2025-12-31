using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using TicketBooking.Application.Common.Interfaces.AI;
using TicketBooking.Infrastructure.AI.Models;

namespace TicketBooking.Infrastructure.AI
{
    public class GeminiEmbeddingService : IAiEmbeddingService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly ILogger<GeminiEmbeddingService> _logger;

        public GeminiEmbeddingService(HttpClient httpClient, IConfiguration configuration, ILogger<GeminiEmbeddingService> logger)
        {
            _httpClient = httpClient;
            // Lấy API Key từ appsettings.json
            _apiKey = configuration["Gemini:ApiKey"] ?? throw new Exception("Gemini API Key is missing!");
            _logger = logger;
        }

        public async Task<float[]> GenerateEmbeddingAsync(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return Array.Empty<float>();

            // URL API của Google (Model text-embedding-004)
            var url = $"https://generativelanguage.googleapis.com/v1beta/models/text-embedding-004:embedContent?key={_apiKey}";

            var request = new GeminiRequest
            {
                Content = new GeminiContent
                {
                    Parts = new List<GeminiPart> { new GeminiPart { Text = text } }
                }
            };

            try
            {
                var response = await _httpClient.PostAsJsonAsync(url, request);

                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    _logger.LogError($"Gemini API Error: {error}");
                    return Array.Empty<float>();
                }

                var result = await response.Content.ReadFromJsonAsync<GeminiResponse>();
                return result?.Embedding.Values ?? Array.Empty<float>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception calling Gemini API");
                return Array.Empty<float>();
            }
        }
    }
}