using FluentValidation;

namespace TicketBooking.Application.Features.Orders.Commands.CreateOrder
{
    // Lớp kiểm tra tính hợp lệ của dữ liệu đầu vào.
    public class CreateOrderCommandValidator : AbstractValidator<CreateOrderCommand>
    {
        public CreateOrderCommandValidator()
        {
            // 1. List không được rỗng
            RuleFor(x => x.Items)
                .NotEmpty().WithMessage("Giỏ hàng không được để trống.");

            // 2. Validate từng item bên trong list
            RuleForEach(x => x.Items).ChildRules(items =>
            {
                items.RuleFor(x => x.TicketTypeId).NotEmpty();
                items.RuleFor(x => x.Quantity)
                    .GreaterThan(0).WithMessage("Số lượng phải lớn hơn 0.");
                    
            });
        }
    }
}