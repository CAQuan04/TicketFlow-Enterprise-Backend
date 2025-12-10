using Microsoft.EntityFrameworkCore;
using TicketBooking.Application.Common.Interfaces;
using TicketBooking.Domain.Entities;
using TicketBooking.Domain.Enums;

namespace TicketBooking.Infrastructure.Data
{
    public class ApplicationDbContext : DbContext, IApplicationDbContext//Thêm kế thừa
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        // Khai báo 6 bảng
        public DbSet<User> Users { get; set; }
        public DbSet<Venue> Venues { get; set; }
        public DbSet<Event> Events { get; set; }
        public DbSet<TicketType> TicketTypes { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<Ticket> Tickets { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // 1. User Configuration
            builder.Entity<User>(e => {
                e.HasKey(u => u.Id);
                e.HasIndex(u => u.Email).IsUnique();

                // QUAN TRỌNG: Cấu hình cho phép PasswordHash được NULL trong Database
                e.Property(u => u.PasswordHash).IsRequired(false);

                e.Property(u => u.Role).HasConversion<string>();
            });

            // 2. Event Configuration
            builder.Entity<Event>(e => {
                e.HasOne(ev => ev.Venue)
                 .WithMany(v => v.Events)
                 .HasForeignKey(ev => ev.VenueId)
                 .OnDelete(DeleteBehavior.Restrict);
            });

            // 3. TicketType Configuration
            builder.Entity<TicketType>(e => {
                e.Property(tt => tt.Price).HasColumnType("decimal(18,2)");
                e.HasOne(tt => tt.Event)
                 .WithMany(ev => ev.TicketTypes)
                 .HasForeignKey(tt => tt.EventId)
                 .OnDelete(DeleteBehavior.Cascade);
                // Cấu hình RowVersion là Concurrency Token
                e.Property(t => t.RowVersion)
                 .IsRowVersion(); // EF Core sẽ dùng cái này để sinh câu lệnh SQL check version.
            });

            // 4. Order Configuration
            builder.Entity<Order>(e => {
                e.Property(o => o.TotalAmount).HasColumnType("decimal(18,2)");
                e.Property(o => o.Status).HasConversion<string>();
                e.HasOne(o => o.User)
                 .WithMany(u => u.Orders)
                 .HasForeignKey(o => o.UserId)
                 .OnDelete(DeleteBehavior.Restrict);
            });

            // 5. Ticket Configuration
            builder.Entity<Ticket>(e => {
                e.HasOne(t => t.Order)
                 .WithMany(o => o.Tickets)
                 .HasForeignKey(t => t.OrderId)
                 .OnDelete(DeleteBehavior.Cascade);

                e.HasOne(t => t.TicketType)
                 .WithMany()
                 .HasForeignKey(t => t.TicketTypeId)
                 .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}