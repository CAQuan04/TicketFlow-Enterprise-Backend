using MediatR;
using TicketBooking.Application.Common.Exceptions; // Sử dụng Custom Exception.
using TicketBooking.Application.Common.Interfaces;
using TicketBooking.Domain.Entities;
using TicketBooking.Domain.Enums;
// Alias để tránh nhầm lẫn với System.ComponentModel.DataAnnotations.ValidationException
using ValidationException = TicketBooking.Application.Common.Exceptions.ValidationException;

namespace TicketBooking.Application.Features.Events.Commands.CreateEvent
{
    public class CreateEventCommandHandler : IRequestHandler<CreateEventCommand, Guid>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUserService;
        public CreateEventCommandHandler(IApplicationDbContext context, ICurrentUserService currentUserService)
        {
            _context = context;
            _currentUserService = currentUserService;
        }

        public async Task<Guid> Handle(CreateEventCommand request, CancellationToken cancellationToken)
        {
            // 1. BUSINESS RULE: VENUE VALIDATION
            // Kiểm tra xem địa điểm tổ chức có tồn tại không.
            var venue = await _context.Venues.FindAsync(new object[] { request.VenueId }, cancellationToken);

            if (venue == null)
            {
                // Nếu không tìm thấy, ném lỗi 404 Not Found.
                throw new NotFoundException(nameof(Venue), request.VenueId);
            }

            // 2. BUSINESS RULE: CAPACITY CHECK (Logic quan trọng)
            // Tính tổng số lượng vé muốn phát hành.
            int totalTicketsRequest = request.TicketTypes.Sum(t => t.Quantity);

            // So sánh với sức chứa tối đa của địa điểm.
            if (totalTicketsRequest > venue.Capacity)
            {
                // CHÍNH THỨC SỬ DỤNG VALIDATION EXCEPTION (HTTP 400)
                // Tạo dictionary lỗi chuẩn.
                var errors = new Dictionary<string, string[]>
                {
                    { "Capacity", new[] { $"Total tickets ({totalTicketsRequest}) exceed venue capacity ({venue.Capacity})." } }
                };

                // Ném ngoại lệ tùy chỉnh (đã được hỗ trợ bởi Constructor thứ 3 sếp vừa thêm).
                throw new ValidationException(errors);
            }

            // 3. MAPPING ENTITY (EVENT)
            // Tạo đối tượng Event.
            var eventEntity = new Event
            {
                Id = Guid.NewGuid(),
                VenueId = request.VenueId,
                Name = request.Name,
                Description = request.Description,
                StartDateTime = request.StartDateTime,
                EndDateTime = request.EndDateTime,
                CoverImageUrl = request.CoverImageUrl,
                CreatedBy = _currentUserService.UserId,
                Status = EventStatus.Draft, // Mặc định là bản nháp.
                CreatedDate = DateTime.UtcNow
            };

            // 4. MAPPING ENTITY (TICKET TYPES - CHILDREN)
            // Duyệt qua danh sách DTO và chuyển đổi sang Entity.
            foreach (var ticketDto in request.TicketTypes)
            {
                var ticketType = new TicketType
                {
                    Id = Guid.NewGuid(),
                    // EventId sẽ được EF Core tự động gán.
                    Name = ticketDto.Name,
                    Price = ticketDto.Price,
                    Quantity = ticketDto.Quantity,
                    AvailableQuantity = ticketDto.Quantity, // Vé còn lại = Tổng vé ban đầu.
                    CreatedDate = DateTime.UtcNow
                };

                // Thêm vào danh sách con của Event.
                eventEntity.TicketTypes.Add(ticketType);
            }

            // 5. PERSISTENCE (TRANSACTIONAL INTEGRITY)
            // EF Core tự động INSERT Event và TicketTypes trong 1 Transaction.
            _context.Events.Add(eventEntity);

            // Lưu xuống Database.
            await _context.SaveChangesAsync(cancellationToken);

            // 6. RETURN RESULT
            return eventEntity.Id;
        }
    }
}