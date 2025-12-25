using TicketBooking.Domain.Common;
using TicketBooking.Domain.Entities;

namespace TicketBooking.Domain.Events
{
    // Sự kiện này bắn ra khi một Event chuyển trạng thái sang Published.
    // Kế thừa BaseEvent (nếu có) hoặc INotification của MediatR (tạm thời để ở App Layer hoặc dùng Wrapper).
    // Ở đây ta dùng cách đơn giản: Class chứa data.
    public class EventPublishedEvent : BaseEvent
    {
        public Event Event { get; }

        public EventPublishedEvent(Event @event)
        {
            Event = @event;
        }
    }
}