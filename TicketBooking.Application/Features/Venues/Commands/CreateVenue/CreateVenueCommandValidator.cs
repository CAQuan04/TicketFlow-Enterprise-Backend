using FluentValidation; // Import FluentValidation library.

namespace TicketBooking.Application.Features.Venues.Commands.CreateVenue
{
    // Validator class to enforce business rules for venue creation.
    public class CreateVenueCommandValidator : AbstractValidator<CreateVenueCommand>
    {
        // Constructor defining the rules.
        public CreateVenueCommandValidator()
        {
            // Rule: Name is mandatory.
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Venue name is required.") // Check for null or empty string.
                .MaximumLength(100).WithMessage("Venue name must not exceed 100 characters."); // Enforce length limit.

            // Rule: Address is mandatory.
            RuleFor(x => x.Address)
                .NotEmpty().WithMessage("Venue address is required."); // Check for null or empty string.

            // Rule: Capacity must be a positive number.
            RuleFor(x => x.Capacity)
                .GreaterThan(0).WithMessage("Capacity must be greater than 0."); // Ensure valid logical capacity.
        }
    }
}