using FluentValidation;

namespace TicketBooking.Application.Features.Events.Commands.CreateEvent
{
    public class CreateEventCommandValidator : AbstractValidator<CreateEventCommand>
    {
        public CreateEventCommandValidator()
        {
            RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
            RuleFor(x => x.Description).NotEmpty();
            RuleFor(x => x.VenueId).NotEmpty();

            // Logic ngày tháng: Ngày bắt đầu phải ở tương lai.
            RuleFor(x => x.StartDateTime)
                .GreaterThan(DateTime.UtcNow).WithMessage("Event start time must be in the future.");

            // Logic ngày tháng: Ngày kết thúc phải sau ngày bắt đầu.
            RuleFor(x => x.EndDateTime)
                .GreaterThan(x => x.StartDateTime).WithMessage("Event end time must be after start time.");

            // Validate danh sách loại vé
            RuleFor(x => x.TicketTypes).NotEmpty().WithMessage("At least one ticket type is required.");

            // Validate từng phần tử trong danh sách vé
            RuleForEach(x => x.TicketTypes).ChildRules(tickets => {
                tickets.RuleFor(t => t.Name).NotEmpty();
                tickets.RuleFor(t => t.Price).GreaterThanOrEqualTo(0);
                tickets.RuleFor(t => t.Quantity).GreaterThan(0);
            });
        }
    }
}