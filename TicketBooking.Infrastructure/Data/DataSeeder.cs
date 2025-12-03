using TicketBooking.Application.Common.Interfaces; // Import logic interface for DbContext.
using TicketBooking.Application.Common.Interfaces.Authentication; // Import interface for Password Hasher.
using TicketBooking.Domain.Constants; // Import Role constants.
using TicketBooking.Domain.Entities; // Import User entity.
using TicketBooking.Domain.Enums; // Import UserRole enum.

namespace TicketBooking.Infrastructure.Data
{
    // Class responsible for seeding initial data into the database.
    public class DataSeeder
    {
        // Dependency for the database context to save data.
        private readonly IApplicationDbContext _context;
        // Dependency for hashing passwords securely (BCrypt).
        private readonly IPasswordHasher _passwordHasher;

        // Constructor injection for dependencies.
        public DataSeeder(IApplicationDbContext context, IPasswordHasher passwordHasher)
        {
            _context = context; // Assign context.
            _passwordHasher = passwordHasher; // Assign hasher.
        }

        // Main async method to execute the seeding logic.
        public async Task SeedAsync()
        {
            // Check if there are already any users in the database to prevent duplicates.
            if (_context.Users.Any())
            {
                // If users exist, do nothing and return.
                return;
            }

            // Define a common password for all seed users (for ease of testing).
            var initialPassword = "Password@123"; // Strong password meeting complexity rules.
            // Hash the password securely using BCrypt before creating user objects.
            var passwordHash = _passwordHasher.Hash(initialPassword);

            // Create a list of predefined users with different roles.
            var users = new List<User>
            {
                // 1. ADMIN USER
                new User
                {
                    Id = Guid.NewGuid(), // Generate unique ID.
                    FullName = "System Administrator", // Set display name.
                    Email = "admin@ticketflow.com", // Set login email.
                    PasswordHash = passwordHash, // Set the SECURE HASH (not plain text).
                    Role = UserRole.Admin, // Assign Admin role.
                    CreatedDate = DateTime.UtcNow // Set creation time.
                },
                // 2. ORGANIZER USER
                new User
                {
                    Id = Guid.NewGuid(), // Generate unique ID.
                    FullName = "Event Organizer", // Set display name.
                    Email = "org@ticketflow.com", // Set login email.
                    PasswordHash = passwordHash, // Set the SECURE HASH.
                    Role = UserRole.Organizer, // Assign Organizer role.
                    CreatedDate = DateTime.UtcNow // Set creation time.
                },
                // 3. CUSTOMER USER
                new User
                {
                    Id = Guid.NewGuid(), // Generate unique ID.
                    FullName = "Regular Customer", // Set display name.
                    Email = "user@ticketflow.com", // Set login email.
                    PasswordHash = passwordHash, // Set the SECURE HASH.
                    Role = UserRole.Customer, // Assign Customer role.
                    CreatedDate = DateTime.UtcNow // Set creation time.
                },
                // 4. TICKET INSPECTOR USER
                new User
                {
                    Id = Guid.NewGuid(), // Generate unique ID.
                    FullName = "Gate Inspector", // Set display name.
                    Email = "inspector@ticketflow.com", // Set login email.
                    PasswordHash = passwordHash, // Set the SECURE HASH.
                    Role = UserRole.TicketInspector, // Assign Inspector role.
                    CreatedDate = DateTime.UtcNow // Set creation time.
                },
                // 5. EVENT MANAGER USER
                new User
                {
                    Id = Guid.NewGuid(), // Generate unique ID.
                    FullName = "Staff Manager", // Set display name.
                    Email = "manager@ticketflow.com", // Set login email.
                    PasswordHash = passwordHash, // Set the SECURE HASH.
                    Role = UserRole.EventManager, // Assign Manager role.
                    CreatedDate = DateTime.UtcNow // Set creation time.
                },
                // 6. FINANCE VIEWER USER
                new User
                {
                    Id = Guid.NewGuid(), // Generate unique ID.
                    FullName = "Accountant", // Set display name.
                    Email = "finance@ticketflow.com", // Set login email.
                    PasswordHash = passwordHash, // Set the SECURE HASH.
                    Role = UserRole.FinanceViewer, // Assign Finance role.
                    CreatedDate = DateTime.UtcNow // Set creation time.
                }
            };

            // Add the list of users to the DbContext in memory.
            _context.Users.AddRange(users);

            // Commit the changes to the SQL Database.
            await _context.SaveChangesAsync(CancellationToken.None);
        }
    }
}