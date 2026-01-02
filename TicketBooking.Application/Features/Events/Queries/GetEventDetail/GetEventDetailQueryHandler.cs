using MediatR;
using Microsoft.EntityFrameworkCore; // Cần thiết cho AsNoTracking, Include.
using TicketBooking.Application.Common.Exceptions; // NotFoundException.
using TicketBooking.Application.Common.Interfaces; // DbContext.
using TicketBooking.Domain.Entities;
using TicketBooking.Domain.Enums;

namespace TicketBooking.Application.Features.Events.Queries.GetEventDetail
{
    public class GetEventDetailQueryHandler : IRequestHandler<GetEventDetailQuery, EventDetailDto>
    {
        private readonly IApplicationDbContext _context;

        public GetEventDetailQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<EventDetailDto> Handle(GetEventDetailQuery request, CancellationToken cancellationToken)
        {
            // 1. QUERY DATABASE WITH PERFORMANCE OPTIMIZATION.
            var eventEntity = await _context.Events
                // ⚠️ CRITICAL PERFORMANCE EXPLANATION (ASNOTRACKING):
                // Standard EF Core queries (Tracking) take a snapshot of data to detect changes for Updates.
                // Since this is a READ-ONLY query, we use AsNoTracking() to bypass the Change Tracker.
                // Benefit: Significantly reduces memory usage and CPU overhead (20-30% faster).
                .AsNoTracking()

                // 2. EAGER LOADING (JOIN).
                // Fetch related Venue data in a single SQL query (avoid N+1 problem).
                .Include(e => e.Venue)
                // Fetch related TicketTypes data.
                .Include(e => e.TicketTypes)

                // 3. FILTERING.
                .FirstOrDefaultAsync(e => e.Id == request.EventId, cancellationToken);

            // 4. VALIDATION: NULL CHECK.
            if (eventEntity == null)
            {
                throw new NotFoundException(nameof(Event), request.EventId);
            }

            // 5. VALIDATION: SECURITY THROUGH OBSCURITY.
            // Only allow viewing "Published" events.
            // If an event exists but is Draft/Cancelled, we act AS IF it doesn't exist (throw NotFound).
            // This prevents attackers from enumerating/guessing Draft event IDs.
            if (eventEntity.Status != EventStatus.Published)
            {
                throw new NotFoundException(nameof(Event), request.EventId);
            }

            // 6. MAPPING TO DTO.
            // Convert the Entity graph to a flat DTO optimized for the Client.
            return new EventDetailDto(
                eventEntity.Id,
                eventEntity.Name,
                eventEntity.Description,
                eventEntity.StartDateTime,
                eventEntity.EndDateTime,
                eventEntity.CoverImageUrl,
                eventEntity.Venue.Name,     // Map Venue Name.
                eventEntity.Venue.Address,  // Map Venue Address.
                eventEntity.Venue.City,
                eventEntity.TicketTypes.Select(t => new TicketTypeDetailDto(
                    t.Id,
                    t.Name,
                    t.Price,
                    t.OriginalPrice,
                    t.Description,                   
                    t.AvailableQuantity
                )).ToList()
            );
        }
    }
}