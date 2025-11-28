using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TicketBooking.Domain.Common;

namespace TicketBooking.Domain.Entities
{
    public class Event : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime EventDate { get; set; }
        public Guid VenueId { get; set; }
        public Venue Venue { get; set; } = null!;
        public ICollection<TicketType> TicketTypes { get; set; } = new List<TicketType>();
    }
}
