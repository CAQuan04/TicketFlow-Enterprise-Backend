using MediatR;
using Microsoft.EntityFrameworkCore;
using TicketBooking.Application.Common.Interfaces;

namespace TicketBooking.Application.Features.Orders.Events
{
    public class OrderPaidEventHandler : INotificationHandler<OrderPaidNotification>
    {
        private readonly IApplicationDbContext _context;
        private readonly IEmailService _emailService;
        private readonly IQrCodeService _qrCodeService;

        public OrderPaidEventHandler(
            IApplicationDbContext context,
            IEmailService emailService,
            IQrCodeService qrCodeService)
        {
            _context = context;
            _emailService = emailService;
            _qrCodeService = qrCodeService;
        }

        public async Task Handle(OrderPaidNotification notification, CancellationToken cancellationToken)
        {
            // 1. Lấy thông tin đơn hàng đầy đủ (bao gồm cả User để lấy email).
            var order = await _context.Orders
                .Include(o => o.Tickets).ThenInclude(t => t.TicketType)
                .Include(o => o.User)
                .AsNoTracking() // Read-only nên dùng NoTracking cho nhanh.
                .FirstOrDefaultAsync(o => o.Id == notification.OrderId, cancellationToken);

            if (order == null) return;

            // 2. Duyệt qua từng vé để gửi mail (Hoặc gửi 1 mail chung chứa nhiều vé).
            // Ở đây demo gửi vé đầu tiên đại diện (hoặc gửi file PDF chứa tất cả QR - nâng cao).
            // Logic đơn giản: Gửi QR của vé đầu tiên.

            var ticket = order.Tickets.First();
            var ticketCode = ticket.TicketCode;

            // 3. GENERATE QR CODE IMAGE (Dùng Service).
            byte[] qrImageBytes = _qrCodeService.GenerateQrCode(ticketCode);

            // 4. CONSTRUCT EMAIL CONTENT.
            string subject = $"[TicketFlow] Your Ticket for Order #{order.OrderCode}";
            string body = $@"
                <h1>Payment Successful!</h1>
                <p>Hi <b>{order.User.FullName}</b>,</p>
                <p>Thank you for purchasing. Here is your ticket details:</p>
                <ul>
                    <li><b>Event Ticket:</b> {ticket.TicketType.Name}</li>
                    <li><b>Ticket Code:</b> {ticketCode}</li>
                    <li><b>Price:</b> {ticket.TicketType.Price:N0} VND</li>
                </ul>
                <p>Please show the attached QR Code at the entrance.</p>
                <br/>
                <p>See you there!</p>
            ";

            // 5. SEND EMAIL WITH ATTACHMENT.
            await _emailService.SendEmailAsync(
                order.User.Email,
                subject,
                body,
                qrImageBytes,
                $"ticket-{ticketCode}.png" // Tên file đính kèm.
            );
        }
    }
}