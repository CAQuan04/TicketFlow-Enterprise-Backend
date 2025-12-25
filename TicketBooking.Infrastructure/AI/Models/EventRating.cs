using Microsoft.ML.Data;

namespace TicketBooking.Infrastructure.AI.Models
{
    // Dữ liệu đầu vào để training: User A thích Event B mức độ nào.
    public class EventRating
    {
        [LoadColumn(0)]
        public string UserId { get; set; } = string.Empty; // Guid string

        [LoadColumn(1)]
        public string EventId { get; set; } = string.Empty; // Guid string

        [LoadColumn(2)]
        public float Label { get; set; } // Điểm số (1 = Click, 5 = Mua, v.v...)
    }
}