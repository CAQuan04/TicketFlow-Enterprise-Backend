using FluentValidation;

namespace TicketBooking.Application.Features.Events.Queries.GetEventsList
{
    // Lớp bảo vệ: Đảm bảo tham số đầu vào an toàn cho hệ thống.
    public class GetEventsListQueryValidator : AbstractValidator<GetEventsListQuery>
    {
        public GetEventsListQueryValidator()
        {
            RuleFor(x => x.PageIndex)
                .GreaterThanOrEqualTo(1).WithMessage("Page index must be at least 1.");

            RuleFor(x => x.PageSize)
                .GreaterThanOrEqualTo(1).WithMessage("Page size must be at least 1.")
                .LessThanOrEqualTo(50).WithMessage("Page size must not exceed 50."); // Chặn tấn công DoS database.
        }
    }
}