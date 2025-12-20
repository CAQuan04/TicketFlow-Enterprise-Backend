using MediatR;
using Microsoft.EntityFrameworkCore;
using TicketBooking.Application.Common.Exceptions;
using TicketBooking.Application.Common.Interfaces;
using TicketBooking.Domain.Entities;
using TicketBooking.Domain.Enums;

namespace TicketBooking.Application.Features.Tickets.Commands.CheckIn
{
    public class CheckInTicketCommandHandler : IRequestHandler<CheckInTicketCommand, CheckInResultDto>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUserService;

        public CheckInTicketCommandHandler(IApplicationDbContext context, ICurrentUserService currentUserService)
        {
            _context = context;
            _currentUserService = currentUserService;
        }

        public async Task<CheckInResultDto> Handle(CheckInTicketCommand request, CancellationToken cancellationToken)
        {
            var inspectorId = _currentUserService.UserId ?? "Unknown";

            // 1. FETCH TICKET (EAGER LOADING)
            // Load Ticket kèm theo thông tin Event, Order và User để validate và hiển thị.
            var ticket = await _context.Tickets
                .Include(t => t.TicketType)
                    .ThenInclude(tt => tt.Event)
                .Include(t => t.Order)
                    .ThenInclude(o => o.User)
                .FirstOrDefaultAsync(t => t.TicketCode == request.TicketCode, cancellationToken);

            // --- FRAUD DETECTION LOGIC (LOGIC PHÁT HIỆN GIAN LẬN) ---

            // CASE 1: FAKE TICKET (Vé giả)
            // Mã vé không tồn tại trong hệ thống.
            if (ticket == null)
            {
                // Ghi log thất bại (Optional: Lưu vào bảng FailedScan nếu cần phân tích sâu).
                throw new NotFoundException("Ticket", request.TicketCode);
            }

            // Tạo bản ghi lịch sử (Audit Trail).
            var history = new CheckInHistory
            {
                Id = Guid.NewGuid(),
                TicketId = ticket.Id,
                InspectorId = inspectorId,
                CheckInTime = DateTime.UtcNow,
                IsSuccess = false // Mặc định là Fail, nếu qua hết check thì set True.
            };

            try
            {
                // CASE 2: TICKET CANCELLED (Vé đã hủy)
                // Vé đã bị hủy do quá hạn thanh toán hoặc hoàn tiền.
                if (ticket.Status == TicketStatus.Cancelled)
                {
                    history.Note = "Failure: Ticket was cancelled.";
                    throw new ValidationException("Vé này đã bị HỦY. Vui lòng kiểm tra lại.");
                }

                // CASE 3: DOUBLE ENTRY (Vào cổng lần 2)
                // Khách đã vào rồi nhưng đưa vé cho người khác vào tiếp.
                if (ticket.Status == TicketStatus.Used)
                {
                    history.Note = $"Failure: Already used at {ticket.CheckedInAt}.";
                    throw new ValidationException($"CẢNH BÁO: Vé đã được sử dụng lúc {ticket.CheckedInAt:HH:mm:ss dd/MM}.");
                }

                // CASE 4: WRONG DATE (Sai ngày)
                // Vé thật nhưng đi nhầm ngày sự kiện (VD: Mua vé ngày 1 nhưng đi ngày 2).
                // Logic: So sánh ngày hiện tại với ngày diễn ra sự kiện.
                // (Ở đây demo so sánh đơn giản, thực tế có thể cho phép vào trước X giờ).
                /* 
                if (ticket.TicketType.Event.StartDateTime.Date != DateTime.UtcNow.Date)
                {
                     history.Note = "Failure: Wrong event date.";
                     throw new ValidationException("Vé không hợp lệ cho ngày hôm nay.");
                }
                */

                // --- SUCCESS PROCESSING ---

                // 1. Update Ticket Status
                ticket.Status = TicketStatus.Used;
                ticket.CheckedInAt = DateTime.UtcNow;

                // 2. Update History
                history.IsSuccess = true;
                history.Note = "Check-in Successful.";

                // 3. Save Changes (Transaction)
                _context.CheckInHistories.Add(history);
                await _context.SaveChangesAsync(cancellationToken);

                // 4. Return Info
                return new CheckInResultDto(
                    ticket.TicketType.Event.Name,
                    ticket.TicketType.Name,
                    ticket.Order.User.FullName,
                    ticket.TicketCode,
                    ticket.CheckedInAt.Value.AddHours(7),
                    "Check-in thành công! Mời quý khách vào cổng."
                );
            }
            catch (Exception)
            {
                // Nếu có lỗi Validation, vẫn lưu lịch sử là Failed để truy vết.
                _context.CheckInHistories.Add(history);
                await _context.SaveChangesAsync(cancellationToken);
                throw; // Ném lại lỗi ra ngoài để Controller trả về 400/404.
            }
        }
    }
}