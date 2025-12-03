using MediatR;
using Microsoft.EntityFrameworkCore;
using TicketBooking.Application.Common.Exceptions;
using TicketBooking.Application.Common.Interfaces;

namespace TicketBooking.Application.Features.Auth.Commands.VerifyEmail
{
    // Handler logic for verification.
    public class VerifyEmailCommandHandler : IRequestHandler<VerifyEmailCommand, string>
    {
        private readonly IApplicationDbContext _context;

        public VerifyEmailCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<string> Handle(VerifyEmailCommand request, CancellationToken cancellationToken)
        {
            // 1. Find user by Email.
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email, cancellationToken);

            // 2. Validate User Existence.
            if (user == null)
            {
                // Throw generic error or NotFound.
                throw new NotFoundException(nameof(Domain.Entities.User), request.Email);
            }

            // 3. Check if OTP matches the stored token.
            if (user.VerificationToken != request.Otp)
            {
                throw new ValidationException(); // Simplified. In real app add specific error "Invalid OTP".
            }

            // 4. Check if OTP has expired.
            if (user.VerificationTokenExpires < DateTime.UtcNow)
            {
                throw new Exception("OTP has expired. Please request a new one.");
            }

            // 5. SUCCESS: Verify the user.
            user.EmailConfirmed = true; // Mark as verified.
            user.VerificationToken = null; // Clear used OTP for security.
            user.VerificationTokenExpires = null; // Clear expiry.

            // 6. Save changes.
            await _context.SaveChangesAsync(cancellationToken);

            return "Email verified successfully! You can now login.";
        }
    }
}