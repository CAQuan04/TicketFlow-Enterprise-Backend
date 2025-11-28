using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TicketBooking.Domain.Common;

namespace TicketBooking.Domain.Entities
{
    public class Venue : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public int Capacity { get; set; }

        public ICollection<Event> Events { get; set; } = new List<Event>();
    }
}
