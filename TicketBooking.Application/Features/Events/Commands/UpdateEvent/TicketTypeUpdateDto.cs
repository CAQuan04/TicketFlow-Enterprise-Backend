namespace TicketBooking.Application.Features.Events.Commands.UpdateEvent
{
    // DTO để nhận dữ liệu vé cần sửa.
    public record TicketTypeUpdateDto(
        Guid? Id,       // Nếu null -> Thêm mới. Nếu có ID -> Sửa/Giữ nguyên.
        string Name,
        decimal Price,
        int Quantity
    );
}