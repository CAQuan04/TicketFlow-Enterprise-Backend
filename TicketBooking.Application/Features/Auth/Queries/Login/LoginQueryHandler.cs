using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MediatR; // Needed for IRequestHandler.
using Microsoft.EntityFrameworkCore; // Needed for Async database methods (FirstOrDefaultAsync).
using TicketBooking.Application.Common.Interfaces; // Needed for IApplicationDbContext.
using TicketBooking.Application.Common.Interfaces.Authentication; // Needed for IPasswordHasher & IJwtTokenGenerator.

namespace TicketBooking.Application.Features.Auth.Queries.Login
{
    // The handler responsible for processing the Login logic.
    public class LoginQueryHandler : IRequestHandler<LoginQuery, string>
    {
        private readonly IApplicationDbContext _context; // Access to the database.
        private readonly IPasswordHasher _passwordHasher; // Service to verify passwords.
        private readonly IJwtTokenGenerator _jwtTokenGenerator; // Service to create tokens.

        // Constructor to inject all necessary dependencies.
        public LoginQueryHandler(
            IApplicationDbContext context,
            IPasswordHasher passwordHasher,
            IJwtTokenGenerator jwtTokenGenerator)
        {
            _context = context; // Initialize DB Context.
            _passwordHasher = passwordHasher; // Initialize Password Hasher.
            _jwtTokenGenerator = jwtTokenGenerator; // Initialize Token Generator.
        }

        // The core logic for authentication.
        public async Task<string> Handle(LoginQuery request, CancellationToken cancellationToken)
        {
            // 1. Database Lookup: Find the user by email address.
            // We use FirstOrDefaultAsync to get the user or null if not found.
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email, cancellationToken);

            // 2. Validation: Check if the user exists in the database.
            if (user == null)
            {
                // Security Best Practice: Don't reveal if Email exists or Password is wrong. Just say "Invalid credentials".
                // In a real app, use a custom ValidationException. Here we throw a generic one for simplicity.
                throw new Exception("Invalid credentials");
            }

            // 3. Validation: Verify if the provided password matches the stored hash.
            // user.PasswordHash might be null (for Google users), verify handles that logic or we check null.
            // Assuming PasswordHash is not null for this flow.
            bool isPasswordValid = _passwordHasher.Verify(request.Password, user.PasswordHash ?? "");

            // 4. Security Check: If password verification fails.
            if (!isPasswordValid)
            {
                // Throw exception to stop execution and deny access.
                throw new Exception("Invalid credentials");
            }

            // 5. Token Generation: Create a JWT for the authenticated user.
            var token = _jwtTokenGenerator.GenerateToken(user);

            // 6. Result: Return the JWT string to the controller.
            return token;
        }
    }
}
