namespace TicketBooking.Infrastructure.Search.Models
{
    // Dữ liệu trên Elastic nên phẳng (Flat) để search nhanh.
    public class EventDocument
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string VenueName { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public decimal MinPrice { get; set; }
        public string? ImageUrl { get; set; }

        public float[]? Embedding { get; set; }
    }
}