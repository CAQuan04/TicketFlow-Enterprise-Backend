using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TicketBooking.Domain.Enums
{
    public enum UserRole
    {
        Customer = 0,       // Can only buy tickets.
        Admin = 1,          // Super user, manages Venues, Users.
        Organizer = 2,      // Can create and manage Events.
        TicketInspector = 3,// Can only scan/check-in tickets.
        EventManager = 4,   // Can help Organizer manage events.
        FinanceViewer = 5   // Can only view reports/dashboard.
    }

}
