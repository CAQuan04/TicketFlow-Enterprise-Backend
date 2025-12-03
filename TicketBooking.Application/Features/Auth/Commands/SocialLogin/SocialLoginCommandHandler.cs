using MediatR; // Import MediatR.
using Microsoft.EntityFrameworkCore; // Import EF Core async methods.
using TicketBooking.Application.Common.Interfaces; // Import DbContext.
using TicketBooking.Application.Common.Interfaces.Authentication; // Import Auth interfaces.
using TicketBooking.Domain.Entities; // Import User Entity.
using TicketBooking.Domain.Enums; // Import Enums.

namespace TicketBooking.Application.Features.Auth.Commands.SocialLogin
{
    // Handler for Social Login logic.
    public class SocialLoginCommandHandler : IRequestHandler<SocialLoginCommand, string>
    {
        private readonly IApplicationDbContext _context; // Access Database.
        private readonly ISocialAuthService _socialAuthService; // Access Social Verification Strategy.
        private readonly IJwtTokenGenerator _jwtTokenGenerator; // Access Token Generator.

        // Constructor Injection.
        public SocialLoginCommandHandler(
            IApplicationDbContext context,
            ISocialAuthService socialAuthService,
            IJwtTokenGenerator jwtTokenGenerator)
        {
            _context = context; // Assign DB.
            _socialAuthService = socialAuthService; // Assign Social Service.
            _jwtTokenGenerator = jwtTokenGenerator; // Assign JWT Generator.
        }

        // Main Handle Method.
        public async Task<string> Handle(SocialLoginCommand request, CancellationToken cancellationToken)
        {
            // 1. STRATEGY SELECTION & VALIDATION.
            // Currently we directly inject the service. 
            // In a multi-provider setup, we would switch based on request.Provider.
            var socialUser = await _socialAuthService.ValidateTokenAsync(request.Token, cancellationToken);

            // 2. CHECK IF USER EXISTS IN DATABASE.
            // We search by Email because email is the unique identifier.
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == socialUser.Email, cancellationToken);

            // 3. LOGIC BRANCHING: LOGIN or REGISTER.
            if (user == null)
            {
                // CASE B: AUTO-REGISTRATION (User does not exist).
                user = new User
                {
                    Id = Guid.NewGuid(), // Generate ID.
                    Email = socialUser.Email, // Set Email.
                    FullName = $"{socialUser.FirstName} {socialUser.LastName}".Trim(), // Set Name.

                    // SECURITY NOTE: Social Users have NO password. 
                    // This prevents them from logging in via standard form unless they set a password later.
                    PasswordHash = null,

                    // SECURITY NOTE: We set EmailConfirmed = true immediately.
                    // Why? Because Google has already verified this email. We trust the Identity Provider.
                    EmailConfirmed = true,

                    Role = UserRole.Customer, // Default Role.
                    CreatedDate = DateTime.UtcNow // Timestamp.
                };

                // Add to DB.
                _context.Users.Add(user);
                await _context.SaveChangesAsync(cancellationToken);
            }
            else
            {
                // CASE A: USER EXISTS (Login).
                // Optional: Update Avatar or Name if changed on Google side.
                // Here we keep it simple and just proceed to login.
            }

            // 4. GENERATE JWT ACCESS TOKEN.
            // Now we treat this user exactly like a standard logged-in user.
            var token = _jwtTokenGenerator.GenerateToken(user);

            // 5. RETURN TOKEN.
            return token;
        }
    }
}