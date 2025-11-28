using MediatR;
using TicketBooking.Domain.Entities;
using TicketBooking.Application.Common.Interfaces; // Sử dụng Interface, không dùng DbContext gốc

namespace TicketBooking.Application.Features.Users.Commands.CreateUser
{
    public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, Guid>
    {
        private readonly IApplicationDbContext _context;

        // Inject Interface vào đây
        public CreateUserCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Guid> Handle(CreateUserCommand request, CancellationToken cancellationToken)
        {
            // 1. Map dữ liệu từ Command sang Entity
            var entity = new User
            {
                Id = Guid.NewGuid(),
                FullName = request.FullName,
                Email = request.Email,
                Role = Domain.Enums.UserRole.Customer, // Mặc định là khách hàng
                CreatedDate = DateTime.UtcNow
                // PasswordHash tạm thời để null hoặc xử lý sau
            };

            // 2. Thêm vào DbContext (thông qua Interface)
            _context.Users.Add(entity);

            // 3. Lưu xuống Database
            await _context.SaveChangesAsync(cancellationToken);

            // 4. Trả về Id
            return entity.Id;
        }
    }
}