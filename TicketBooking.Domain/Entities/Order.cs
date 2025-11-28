using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using TicketBooking.Domain.Common;
using TicketBooking.Domain.Enums;

namespace TicketBooking.Domain.Entities
{
    public class Order : BaseEntity
    {
        public Guid UserId { get; set; }
        public User User { get; set; } = null!;

        public string OrderCode { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public OrderStatus Status { get; set; } = OrderStatus.Pending;
        public DateTime OrderDate { get; set; } = DateTime.UtcNow;

        public ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
    }
}
