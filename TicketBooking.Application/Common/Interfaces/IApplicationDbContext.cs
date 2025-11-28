using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TicketBooking.Domain.Entities;

namespace TicketBooking.Application.Common.Interfaces
{
    public interface IApplicationDbContext
    {
        DbSet<User> Users { get; }
        DbSet<Event> Events { get; }
        DbSet<Order> Orders { get; }
        DbSet<Ticket> Tickets { get; }
        DbSet<Venue> Venues { get; }
        DbSet<TicketType> TicketTypes { get; }

        Task<int> SaveChangesAsync(CancellationToken cancellationToken);

    }
}
