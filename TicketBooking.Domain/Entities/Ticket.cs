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
    public class Ticket : BaseEntity
    {
        public Guid OrderId { get; set; }
        public Order Order { get; set; } = null!;

        public Guid TicketTypeId { get; set; }
        public TicketType TicketType { get; set; } = null!;

        public string TicketCode { get; set; } = string.Empty;

        // Trạng thái vé.
        public TicketStatus Status { get; set; } = TicketStatus.Active;

        // Thời điểm Check-in (Nullable vì lúc đầu chưa vào).
        public DateTime? CheckedInAt { get; set; }

    }
}
