using FluentValidation; // Import FluentValidation.

namespace TicketBooking.Application.Features.Events.Commands.CreateEvent
{
    // Validator for creating an event.
    public class CreateEventCommandValidator : AbstractValidator<CreateEventCommand>
    {
        public CreateEventCommandValidator()
        {
            // Rule: VenueId must be provided.
            RuleFor(x => x.VenueId)
                .NotEmpty().WithMessage("Venue ID is required."); // Ensure GUID is not empty.

            // Rule: Name is required.
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Event name is required."); // Check for empty string.

            // Rule: EventDate must be in the future.
            RuleFor(x => x.EventDate)
                .GreaterThan(DateTime.UtcNow).WithMessage("Event date must be in the future."); // Business logic rule.
        }
    }
}