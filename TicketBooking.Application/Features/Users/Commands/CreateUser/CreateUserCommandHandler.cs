using MediatR; // Needed for IRequestHandler.
using TicketBooking.Domain.Entities; // Needed for User entity.
using TicketBooking.Application.Common.Interfaces; // Needed for IApplicationDbContext.
using TicketBooking.Application.Common.Interfaces.Authentication; // Needed for IPasswordHasher.

namespace TicketBooking.Application.Features.Users.Commands.CreateUser
{
    // The handler class implementing the logic for the CreateUserCommand.
    public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, Guid>
    {
        private readonly IApplicationDbContext _context; // Dependency to access database tables.
        private readonly IPasswordHasher _passwordHasher; // Dependency to hash passwords securely.

        // Constructor injection to get the required services.
        public CreateUserCommandHandler(IApplicationDbContext context, IPasswordHasher passwordHasher)
        {
            _context = context; // Assign the injected context to the private field.
            _passwordHasher = passwordHasher; // Assign the injected hasher to the private field.
        }

        // The main method that handles the command logic.
        public async Task<Guid> Handle(CreateUserCommand request, CancellationToken cancellationToken)
        {
            // 1. Security: Hash the raw password before doing anything else.
            // Never save raw passwords in the database to prevent leaks.
            var hashedPassword = _passwordHasher.Hash(request.Password);

            // 2. Create the User Entity object.
            var entity = new User
            {
                Id = Guid.NewGuid(), // Generate a unique identifier for the new user.
                FullName = request.FullName, // Map the FullName from the request.
                Email = request.Email, // Map the Email from the request.
                PasswordHash = hashedPassword, // Assign the SECURE HASH string, not the raw password.
                Role = Domain.Enums.UserRole.Customer, // Default role is Customer.
                CreatedDate = DateTime.UtcNow // Set the creation timestamp to UTC.
            };

            // 3. Persistence: Add the new entity to the Users DbSet in memory.
            _context.Users.Add(entity);

            // 4. Commit: Save changes to the actual SQL Database asynchronously.
            await _context.SaveChangesAsync(cancellationToken);

            // 5. Return: Send back the ID of the newly created user.
            return entity.Id;
        }
    }
}