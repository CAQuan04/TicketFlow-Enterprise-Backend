using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
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
        DbSet<Wallet> Wallets { get; }
        DbSet<WalletTransaction> WalletTransactions { get; }
        Task<int> SaveChangesAsync(CancellationToken cancellationToken);

        // THÊM DÒNG NÀY: Để truy cập Transaction
        DatabaseFacade Database { get; }

    }
}
