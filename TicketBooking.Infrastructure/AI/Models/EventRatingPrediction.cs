namespace TicketBooking.Infrastructure.AI.Models
{
    // Kết quả dự đoán từ AI.
    public class EventRatingPrediction
    {
        public float Score { get; set; } // Điểm số dự đoán (Càng cao càng match).
    }
}