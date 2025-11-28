using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TicketBooking.Domain.Common;

namespace TicketBooking.Domain.Entities
{
    public class TicketType : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public int AvailableQuantity { get; set; }

        public Guid EventId { get; set; }
        public Event Event { get; set; } = null!;
    }
}
