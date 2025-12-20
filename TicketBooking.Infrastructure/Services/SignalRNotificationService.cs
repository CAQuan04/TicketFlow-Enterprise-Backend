using Microsoft.AspNetCore.SignalR;
using TicketBooking.Application.Common.Interfaces.RealTime;
using TicketBooking.Infrastructure.Hubs; // Ở Infra nên nhìn thấy Hub thoải mái

namespace TicketBooking.Infrastructure.Services
{
    public class SignalRNotificationService : INotificationService
    {
        private readonly IHubContext<NotificationHub, INotificationClient> _hubContext;

        public SignalRNotificationService(IHubContext<NotificationHub, INotificationClient> hubContext)
        {
            _hubContext = hubContext;
        }

        public async Task SendToUserAsync(string userId, string message)
        {
            // Gọi hàm ReceiveNotification của Client
            await _hubContext.Clients.User(userId).ReceiveNotification(message);
        }

        public async Task SendToGroupAsync(string groupName, string messageType, object data)
        {
            if (messageType == "UpdateInventory")
            {
                // Gọi hàm UpdateInventory của Client (data ở đây là số lượng vé)
                await _hubContext.Clients.Group(groupName).UpdateInventory(Guid.Parse(groupName), (int)data);
            }
        }
    }
}