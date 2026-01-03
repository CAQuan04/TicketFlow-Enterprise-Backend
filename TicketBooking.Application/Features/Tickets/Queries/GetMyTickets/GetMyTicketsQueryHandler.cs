using MediatR;
using Microsoft.EntityFrameworkCore;
using TicketBooking.Application.Common.Interfaces;
using TicketBooking.Domain.Enums;

namespace TicketBooking.Application.Features.Tickets.Queries.GetMyTickets
{
    public class GetMyTicketsQueryHandler : IRequestHandler<GetMyTicketsQuery, List<MyTicketDto>>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUserService;

        public GetMyTicketsQueryHandler(IApplicationDbContext context, ICurrentUserService currentUserService)
        {
            _context = context;
            _currentUserService = currentUserService;
        }

        public async Task<List<MyTicketDto>> Handle(GetMyTicketsQuery request, CancellationToken cancellationToken)
        {
            // 1. Lấy ID user đang đăng nhập
            var userId = Guid.Parse(_currentUserService.UserId!);

            // 2. Query Database (EF Core Projection)
            // Chúng ta dùng Select để map trực tiếp từ DB sang DTO.
            // Cách này nhanh hơn AutoMapper vì nó chỉ lấy đúng cột cần thiết từ SQL.
            var tickets = await _context.Tickets
                .AsNoTracking() // Read-only optimization
                .Where(t => t.Order.UserId == userId
                         && t.Order.Status == OrderStatus.Paid) // Chỉ lấy vé của đơn đã thanh toán thành công
                .OrderByDescending(t => t.CreatedDate) // Vé mới mua lên đầu
                .Select(t => new MyTicketDto(
                    t.Id,
                    t.TicketCode,
                    t.TicketType.Event.Name,
                    t.TicketType.Event.Venue.Name,
                    t.TicketType.Event.Venue.Address,
                    t.TicketType.Event.StartDateTime,
                    t.TicketType.Name,
                    t.TicketType.Price,
                    t.Status.ToString(), // Convert Enum sang String cho FE dễ dùng
                    t.OrderId,
                    t.TicketType.Event.CoverImageUrl
                ))
                .ToListAsync(cancellationToken);

            return tickets;
        }
    }
}