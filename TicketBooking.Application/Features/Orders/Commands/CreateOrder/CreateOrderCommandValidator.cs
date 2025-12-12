using FluentValidation;

namespace TicketBooking.Application.Features.Orders.Commands.CreateOrder
{
    // Lớp kiểm tra tính hợp lệ của dữ liệu đầu vào.
    public class CreateOrderCommandValidator : AbstractValidator<CreateOrderCommand>
    {
        public CreateOrderCommandValidator()
        {
            // TicketTypeId bắt buộc phải có.
            RuleFor(x => x.TicketTypeId).NotEmpty().WithMessage("Ticket Type is required.");

            // Số lượng phải từ 1 trở lên.
            RuleFor(x => x.Quantity)
                .GreaterThan(0).WithMessage("Quantity must be at least 1.")
                .LessThanOrEqualTo(10).WithMessage("You can only buy max 10 tickets at once."); // Rule nghiệp vụ chặn mua sỉ.
        }
    }
}