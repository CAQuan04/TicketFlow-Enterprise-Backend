using MediatR;
using Microsoft.EntityFrameworkCore;
using TicketBooking.Application.Common.Exceptions;
using TicketBooking.Application.Common.Interfaces;
using TicketBooking.Domain.Entities;

namespace TicketBooking.Application.Features.Orders.Queries.GetOrderById
{
    public class GetOrderByIdQueryHandler : IRequestHandler<GetOrderByIdQuery, OrderDto>
    {
        private readonly IApplicationDbContext _context;

        public GetOrderByIdQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<OrderDto> Handle(GetOrderByIdQuery request, CancellationToken cancellationToken)
        {
            // Query đơn giản, dùng AsNoTracking để tối ưu hiệu năng Read
            var order = await _context.Orders
                .AsNoTracking()
                .FirstOrDefaultAsync(o => o.Id == request.OrderId, cancellationToken);

            if (order == null) throw new NotFoundException(nameof(Order), request.OrderId);

            return new OrderDto(
                order.Id,
                order.OrderCode,
                order.TotalAmount,
                order.Status.ToString(),
                order.CreatedDate
            );
        }
    }
}