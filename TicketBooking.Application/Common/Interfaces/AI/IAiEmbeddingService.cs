namespace TicketBooking.Application.Common.Interfaces.AI
{
    public interface IAiEmbeddingService
    {
        // Hàm biến đổi văn bản (string) thành Vector (mảng số float).
        // Gemini Pro trả về mảng 768 số.
        Task<float[]> GenerateEmbeddingAsync(string text);
    }
}