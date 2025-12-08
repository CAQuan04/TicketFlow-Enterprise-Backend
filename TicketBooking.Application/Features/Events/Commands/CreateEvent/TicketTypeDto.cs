namespace TicketBooking.Application.Features.Events.Commands.CreateEvent
{
    // DTO con, dùng để định nghĩa danh sách các loại vé trong lệnh tạo sự kiện.
    public record TicketTypeDto(
        string Name,    // Tên loại vé.
        decimal Price,  // Giá tiền.
        int Quantity    // Số lượng phát hành.
    );
}