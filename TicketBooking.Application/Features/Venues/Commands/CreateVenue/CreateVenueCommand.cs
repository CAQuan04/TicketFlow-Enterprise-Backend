using MediatR; // Import MediatR library to implement IRequest.

namespace TicketBooking.Application.Features.Venues.Commands.CreateVenue
{
    // Define a record to encapsulate the request data for creating a venue.
    // It implements IRequest<Guid> because we expect to return the new Venue's ID.
    public record CreateVenueCommand(
        string Name, // The name of the venue (e.g., "My Dinh Stadium").
        string Address, // The physical address of the venue.
        int Capacity // The maximum number of people allowed.
    ) : IRequest<Guid>;
}