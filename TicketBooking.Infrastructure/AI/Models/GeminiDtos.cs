using System.Text.Json.Serialization;

namespace TicketBooking.Infrastructure.AI.Models
{
    // Request gửi đi
    public class GeminiRequest
    {
        [JsonPropertyName("model")]
        public string Model { get; set; } = "models/text-embedding-004";
        [JsonPropertyName("content")]
        public GeminiContent Content { get; set; } = new();
    }

    public class GeminiContent
    {
        [JsonPropertyName("parts")]
        public List<GeminiPart> Parts { get; set; } = new();
    }

    public class GeminiPart
    {
        [JsonPropertyName("text")]
        public string Text { get; set; } = string.Empty;
    }

    // Response nhận về
    public class GeminiResponse
    {
        [JsonPropertyName("embedding")]
        public GeminiEmbeddingValues Embedding { get; set; } = new();
    }

    public class GeminiEmbeddingValues
    {
        [JsonPropertyName("values")]
        public float[] Values { get; set; } = Array.Empty<float>();
    }
}