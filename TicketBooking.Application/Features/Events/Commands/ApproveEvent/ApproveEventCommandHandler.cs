using MediatR;
using TicketBooking.Application.Common.Exceptions; // Custom Exceptions.
using TicketBooking.Application.Common.Interfaces; // DbContext Interface.
using TicketBooking.Domain.Entities; // Event Entity.
using TicketBooking.Domain.Enums;
using TicketBooking.Domain.Events; // EventStatus Enum.

namespace TicketBooking.Application.Features.Events.Commands.ApproveEvent
{
    public class ApproveEventCommandHandler : IRequestHandler<ApproveEventCommand>
    {
        private readonly IApplicationDbContext _context;

        private readonly IPublisher _publisher;


        public ApproveEventCommandHandler(IApplicationDbContext context,
            IPublisher publisher)
        {
            _context = context;
            _publisher = publisher;
        }

        public async Task Handle(ApproveEventCommand request, CancellationToken cancellationToken)
        {
            // 1. FETCH EVENT FROM DB.
            var eventEntity = await _context.Events.FindAsync(new object[] { request.EventId }, cancellationToken);

            // 2. CHECK EXISTENCE.
            if (eventEntity == null)
            {
                // Throw 404 if ID is invalid.
                throw new NotFoundException(nameof(Event), request.EventId);
            }

            // 3. BUSINESS RULE: CHECK CURRENT STATUS (IDEMPOTENCY & LOGIC).
            // Why check? 
            // A. Idempotency: If the API is called twice by mistake, we shouldn't process it again.
            // B. Logic: You cannot "Approve" an event that is already Cancelled or Completed.
            if (eventEntity.Status == EventStatus.Published)
            {
                // Throw error to inform the Admin that this action is redundant.
                throw new ValidationException(); // Hoặc throw new Exception("Event is already published.");
            }

            // Further check: Should we allow approving a Cancelled event? Usually NO.
            if (eventEntity.Status == EventStatus.Cancelled)
            {
                throw new Exception("Cannot approve a cancelled event.");
            }

            // 4. UPDATE STATUS.
            // Change status to Published so it becomes visible to Customers.
            eventEntity.Status = EventStatus.Published;

            // 5. PERSISTENCE.
            // EF Core tracks the change and generates an SQL UPDATE statement.
            await _context.SaveChangesAsync(cancellationToken);

            // (Optional Idea for Sếp): Here we could trigger an Email Notification to the Organizer 
            // saying "Your event has been approved!".

            // Bắn sự kiện để các bên khác (như Elastic Sync) biết mà làm việc.
            
            await _publisher.Publish(new EventPublishedEvent(eventEntity));
            await _publisher.Publish(new Domain.Events.EventPublishedEvent(eventEntity), cancellationToken);
        }
    }
}