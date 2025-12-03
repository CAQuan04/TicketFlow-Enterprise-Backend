using MediatR; // Import MediatR.
using TicketBooking.Application.Common.Exceptions; // Import custom exceptions.
using TicketBooking.Application.Common.Interfaces; // Import interfaces.
using TicketBooking.Domain.Entities; // Import entities.

namespace TicketBooking.Application.Features.Events.Commands.CreateEvent
{
    // Handler for CreateEventCommand.
    public class CreateEventCommandHandler : IRequestHandler<CreateEventCommand, Guid>
    {
        private readonly IApplicationDbContext _context; // Database access.

        public CreateEventCommandHandler(IApplicationDbContext context)
        {
            _context = context; // Inject context.
        }

        public async Task<Guid> Handle(CreateEventCommand request, CancellationToken cancellationToken)
        {
            // 1. VERIFY VENUE EXISTENCE (Business Logic).
            // Attempt to find the Venue in the database using the provided ID.
            var venue = await _context.Venues.FindAsync(new object[] { request.VenueId }, cancellationToken);

            // If the venue does not exist, we cannot link the event.
            if (venue == null)
            {
                // Throw our custom NotFoundException.
                // The GlobalExceptionHandler in the API layer will catch this and return a 404 Not Found response.
                throw new NotFoundException(nameof(Venue), request.VenueId);
            }

            // 2. Create the Event Entity.
            var entity = new Event
            {
                Id = Guid.NewGuid(), // Generate ID.
                VenueId = request.VenueId, // Link to the existing Venue.
                Name = request.Name, // Map Name.
                Description = request.Description, // Map Description.
                EventDate = request.EventDate, // Map Date.
                CreatedDate = DateTime.UtcNow // Set timestamp.
            };

            // 3. Persistence.
            _context.Events.Add(entity); // Add to DbContext.
            await _context.SaveChangesAsync(cancellationToken); // Commit transaction.

            // Return the new Event ID.
            return entity.Id;
        }
    }
}