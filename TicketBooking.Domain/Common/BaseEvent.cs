using MediatR; // Bắt buộc phải có để dùng INotification

namespace TicketBooking.Domain.Common
{
    // Class cha cho các Sự kiện (Domain Events).
    // Kế thừa INotification để MediatR có thể bắn và nhận được.
    public abstract class BaseEvent : INotification
    {
        public DateTime DateOccurred { get; protected set; } = DateTime.UtcNow;
    }
}