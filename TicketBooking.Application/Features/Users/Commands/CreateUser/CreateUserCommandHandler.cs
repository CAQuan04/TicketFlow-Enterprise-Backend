using MediatR; // Needed for IRequestHandler.
using TicketBooking.Domain.Entities; // Needed for User entity.
using TicketBooking.Application.Common.Interfaces; // Needed for IApplicationDbContext.
using TicketBooking.Application.Common.Interfaces.Authentication; // Needed for IPasswordHasher.

namespace TicketBooking.Application.Features.Users.Commands.CreateUser
{
    // The handler class implementing the logic for the CreateUserCommand.
    public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, string>
    {
        private readonly IApplicationDbContext _context; // Dependency to access database tables.
        private readonly IPasswordHasher _passwordHasher; // Dependency to hash passwords securely.
        private readonly IEmailService _emailService; // Inject Email Service.

        // Constructor injection to get the required services.
        public CreateUserCommandHandler(IApplicationDbContext context, IPasswordHasher passwordHasher,
        IEmailService emailService)
        {
            _context = context; // Assign the injected context to the private field.
            _passwordHasher = passwordHasher; // Assign the injected hasher to the private field.
            _emailService = emailService;
        }

        // The main method that handles the command logic.
        public async Task<string> Handle(CreateUserCommand request, CancellationToken cancellationToken)
        {
            // 1. Security: Hash the raw password before doing anything else.
            // Never save raw passwords in the database to prevent leaks.
            var hashedPassword = _passwordHasher.Hash(request.Password);

            // 1. GENERATE OTP (6 digits).
            var random = new Random();
            var otp = random.Next(100000, 999999).ToString();

            // 2. Create the User Entity object.
            var entity = new User
            {
                Id = Guid.NewGuid(), // Generate a unique identifier for the new user.
                FullName = request.FullName, // Map the FullName from the request.
                Email = request.Email, // Map the Email from the request.
                PasswordHash = hashedPassword, // Assign the SECURE HASH string, not the raw password.
                Role = Domain.Enums.UserRole.Customer, // Default role is Customer.
                CreatedDate = DateTime.UtcNow, // Set the creation timestamp to UTC.

                // 2. SET VERIFICATION DATA.
                VerificationToken = otp, // Save OTP.
                VerificationTokenExpires = DateTime.UtcNow.AddMinutes(15), // Expire in 15 mins.
                EmailConfirmed = false // Not verified yet.
            };

            // 3. Persistence: Add the new entity to the Users DbSet in memory.
            _context.Users.Add(entity);

            // 4. Commit: Save changes to the actual SQL Database asynchronously.
            await _context.SaveChangesAsync(cancellationToken);

            // 3. SEND EMAIL ASYNCHRONOUSLY.
            // We use Fire-and-Forget style or await it. Here we await to ensure it's sent.
            var emailSubject = "Verify your TicketFlow Account";
            var emailBody = $"<h3>Welcome to TicketFlow!</h3><p>Your verification code is: <b>{otp}</b></p>";

            await _emailService.SendEmailAsync(request.Email, emailSubject, emailBody);

            // 4. RETURN SUCCESS MESSAGE.
            return $"User created with ID {entity.Id}. OTP sent to {request.Email}. Please verify.";
        }
    }
}