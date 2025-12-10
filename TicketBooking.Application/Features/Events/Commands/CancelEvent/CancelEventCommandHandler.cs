using MediatR;
using Microsoft.EntityFrameworkCore;
using TicketBooking.Application.Common.Exceptions;
using TicketBooking.Application.Common.Interfaces;
using TicketBooking.Domain.Entities;
using TicketBooking.Domain.Enums;

namespace TicketBooking.Application.Features.Events.Commands.CancelEvent
{
    public class CancelEventCommandHandler : IRequestHandler<CancelEventCommand>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUserService;

        public CancelEventCommandHandler(IApplicationDbContext context, ICurrentUserService currentUserService)
        {
            _context = context;
            _currentUserService = currentUserService;
        }

        public async Task Handle(CancelEventCommand request, CancellationToken cancellationToken)
        {
            // 1. LẤY EVENT
            var eventEntity = await _context.Events.FindAsync(new object[] { request.EventId }, cancellationToken);
            if (eventEntity == null) throw new NotFoundException(nameof(Event), request.EventId);

            // 2. CHECK TRẠNG THÁI
            if (eventEntity.Status == EventStatus.Cancelled) return; // Đã hủy rồi thì thôi.

            // 3. SECURITY CHECK: OWNER HOẶC ADMIN
            var currentUserId = _currentUserService.UserId;
            bool isOwner = eventEntity.CreatedBy == currentUserId;

            // Truy vấn DB để xem User hiện tại có phải Admin không.
            var currentUser = await _context.Users.FindAsync(new object[] { Guid.Parse(currentUserId!) }, cancellationToken);
            bool isAdmin = currentUser?.Role == UserRole.Admin;

            // Nếu không phải Chủ, cũng không phải Admin -> Chặn.
            if (!isOwner && !isAdmin)
            {
                throw new UnauthorizedAccessException("You do not have permission to cancel this event.");
            }

            // 4. UPDATE TRẠNG THÁI
            eventEntity.Status = EventStatus.Cancelled;
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}