using TicketBooking.Domain.Common;

namespace TicketBooking.Domain.Entities
{
    public class Wallet : BaseEntity
    {
        public Guid UserId { get; set; }

        // Navigation Property: Link tới User (Optional, tùy Sếp có muốn load User từ Wallet không)
        // public User User { get; set; } = null!; 

        // QUAN TRỌNG: Dùng decimal cho tiền tệ. Tuyệt đối không dùng double/float.
        public decimal Balance { get; set; } = 0;

        public string Currency { get; set; } = "VND";

        // CONCURRENCY TOKEN: Chống Race Condition khi cộng/trừ tiền.
        public byte[] RowVersion { get; set; } = null!;
    }
}