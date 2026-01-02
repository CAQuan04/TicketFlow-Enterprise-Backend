using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TicketBooking.Domain.Entities;

namespace TicketBooking.Infrastructure.Data.Configurations
{
    public class VenueConfiguration : IEntityTypeConfiguration<Venue>
    {
        public void Configure(EntityTypeBuilder<Venue> builder)
        {
            builder.Property(v => v.Name).IsRequired().HasMaxLength(200);
            builder.Property(v => v.Address).IsRequired().HasMaxLength(500);
            builder.Property(v => v.City).HasMaxLength(100).HasDefaultValue("");
        }
    }
}