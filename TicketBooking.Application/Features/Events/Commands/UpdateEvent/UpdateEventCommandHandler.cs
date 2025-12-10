using MediatR;
using Microsoft.EntityFrameworkCore;
using TicketBooking.Application.Common.Exceptions;
using TicketBooking.Application.Common.Interfaces;
using TicketBooking.Domain.Entities;
using TicketBooking.Domain.Enums;

namespace TicketBooking.Application.Features.Events.Commands.UpdateEvent
{
    public class UpdateEventCommandHandler : IRequestHandler<UpdateEventCommand>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUserService;

        public UpdateEventCommandHandler(IApplicationDbContext context, ICurrentUserService currentUserService)
        {
            _context = context;
            _currentUserService = currentUserService;
        }

        public async Task Handle(UpdateEventCommand request, CancellationToken cancellationToken)
        {
            // 1. LẤY EVENT VÀ TICKET TYPES TỪ DB
            var eventEntity = await _context.Events
                .Include(e => e.TicketTypes) // Eager Loading để so sánh.
                .FirstOrDefaultAsync(e => e.Id == request.EventId, cancellationToken);

            if (eventEntity == null) throw new NotFoundException(nameof(Event), request.EventId);

            // 2. SECURITY CHECK: CHÍNH CHỦ (OWNERSHIP)
            var currentUserId = _currentUserService.UserId;
            if (eventEntity.CreatedBy != currentUserId)
            {
                // Nếu người đang login không phải người tạo -> Chặn.
                throw new UnauthorizedAccessException("You are not the owner of this event.");
            }

            // 3. LOGIC: KHÔNG ĐƯỢC SỬA SỰ KIỆN ĐÃ HỦY
            if (eventEntity.Status == EventStatus.Cancelled)
            {
                throw new Exception("Cannot update a cancelled event.");
            }

            // 4. UPDATE THÔNG TIN CƠ BẢN
            eventEntity.Name = request.Name;
            eventEntity.Description = request.Description;
            eventEntity.CoverImageUrl = request.CoverImageUrl;
            eventEntity.StartDateTime = request.StartDateTime;
            eventEntity.EndDateTime = request.EndDateTime;

            // 5. UPDATE TICKET TYPES (PHỨC TẠP)
            var existingTypes = eventEntity.TicketTypes.ToList();
            var requestTypes = request.TicketTypes;

            // A. XÓA (Delete): Có trong DB nhưng không có trong Request.
            var typesToDelete = existingTypes.Where(e => !requestTypes.Any(r => r.Id == e.Id)).ToList();
            foreach (var type in typesToDelete)
            {
                // Logic an toàn: Chỉ xóa nếu chưa bán được vé nào.
                if (type.Quantity != type.AvailableQuantity)
                {
                    throw new Exception($"Cannot delete ticket type '{type.Name}' because tickets have been sold.");
                }
                _context.TicketTypes.Remove(type);
            }

            // B. SỬA (Update): Có ID khớp nhau.
            foreach (var reqType in requestTypes.Where(x => x.Id.HasValue))
            {
                var existing = existingTypes.FirstOrDefault(x => x.Id == reqType.Id);
                if (existing != null)
                {
                    existing.Name = reqType.Name;
                    existing.Price = reqType.Price;

                    // Logic Update Số lượng:
                    int difference = reqType.Quantity - existing.Quantity; // Mới - Cũ.
                    existing.Quantity = reqType.Quantity;
                    existing.AvailableQuantity += difference; // Cộng/Trừ vào số khả dụng.

                    // Check an toàn: Không được giảm xuống dưới mức đã bán.
                    if (existing.AvailableQuantity < 0)
                    {
                        throw new Exception($"Cannot reduce quantity of '{existing.Name}' below sold amount.");
                    }
                }
            }

            // C. THÊM (Add): Không có ID hoặc ID null.
            foreach (var reqType in requestTypes.Where(x => !x.Id.HasValue || x.Id == Guid.Empty))
            {
                eventEntity.TicketTypes.Add(new TicketType
                {
                    Id = Guid.NewGuid(),
                    Name = reqType.Name,
                    Price = reqType.Price,
                    Quantity = reqType.Quantity,
                    AvailableQuantity = reqType.Quantity // Mới tạo thì Full.
                });
            }

            // 6. LƯU DATABASE
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}