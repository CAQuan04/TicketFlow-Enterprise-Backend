using MediatR; // Import MediatR.
using TicketBooking.Application.Common.Interfaces; // Import application interfaces.
using TicketBooking.Domain.Entities; // Import domain entities.

namespace TicketBooking.Application.Features.Venues.Commands.CreateVenue
{
    // Handler responsible for processing the CreateVenueCommand.
    public class CreateVenueCommandHandler : IRequestHandler<CreateVenueCommand, Guid>
    {
        private readonly IApplicationDbContext _context; // Dependency to interact with the database.

        // Constructor injection.
        public CreateVenueCommandHandler(IApplicationDbContext context)
        {
            _context = context; // Assign the injected context.
        }

        // The core logic method.
        public async Task<Guid> Handle(CreateVenueCommand request, CancellationToken cancellationToken)
        {
            // Create a new Venue entity and map data from the command.
            var venue = new Venue
            {
                Id = Guid.NewGuid(), // Generate a unique ID.
                Name = request.Name, // Map Name.
                Address = request.Address, // Map Address.
                Capacity = request.Capacity, // Map Capacity.
                CreatedDate = DateTime.UtcNow // Set creation timestamp.
            };

            // Add the new entity to the DbSet in memory.
            _context.Venues.Add(venue);

            // Save changes to the database asynchronously.
            await _context.SaveChangesAsync(cancellationToken);

            // Return the ID of the newly created venue.
            return venue.Id;
        }
    }
}