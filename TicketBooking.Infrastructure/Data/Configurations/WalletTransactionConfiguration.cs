using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TicketBooking.Domain.Entities;

namespace TicketBooking.Infrastructure.Data.Configurations
{
    public class WalletTransactionConfiguration : IEntityTypeConfiguration<WalletTransaction>
    {
        public void Configure(EntityTypeBuilder<WalletTransaction> builder)
        {
            builder.Property(t => t.Amount).HasColumnType("decimal(18,2)");

            // Index để query lịch sử giao dịch nhanh hơn.
            builder.HasIndex(t => t.WalletId);
            builder.HasIndex(t => t.ReferenceId);
        }
    }
}