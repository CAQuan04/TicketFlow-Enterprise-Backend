using FluentValidation; // Import FluentValidation.

namespace TicketBooking.Application.Features.Users.Commands.CreateUser
{
    // Validator class for CreateUserCommand. Inherits from AbstractValidator.
    public class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
    {
        public CreateUserCommandValidator()
        {
            // Rule for FullName: Must not be empty and strictly max 100 characters.
            RuleFor(x => x.FullName)
                .NotEmpty().WithMessage("Full Name is required.") // Error if null/empty.
                .MaximumLength(100).WithMessage("Full Name must not exceed 100 characters."); // Error if too long.

            // Rule for Email: Must not be empty and must match standard email format.
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required.") // Error if null/empty.
                .EmailAddress().WithMessage("A valid email address is required."); // Validates format (e.g. user@domain.com).

            // STRICT PASSWORD RULES: Security Requirement.
            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Password is required.") // 1. Not empty.
                .MinimumLength(8).WithMessage("Password must be at least 8 characters long.") // 2. Min Length.
                .MaximumLength(32).WithMessage("Password must not exceed 32 characters.") // 3. Max Length.
                                                                                          // Regex: At least one uppercase letter (A-Z).
                .Matches(@"[A-Z]").WithMessage("Password must contain at least one uppercase letter.")
                // Regex: At least one lowercase letter (a-z).
                .Matches(@"[a-z]").WithMessage("Password must contain at least one lowercase letter.")
                // Regex: At least one digit (0-9).
                .Matches(@"[0-9]").WithMessage("Password must contain at least one number.")
                // Regex: At least one special character (!, @, #, etc.).
                // [^\w\d] checks for any character that is NOT a word char or digit.
                .Matches(@"[\!\?\*\.\@\#\$\%\^\&\(\)\-\+\=\<\>\,\/\[\]\{\}\|]").WithMessage("Password must contain at least one special character (!? *.@#$%^& etc).");
        }
    }
}