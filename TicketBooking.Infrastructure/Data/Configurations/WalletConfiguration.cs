using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TicketBooking.Domain.Entities;

namespace TicketBooking.Infrastructure.Data.Configurations
{
    public class WalletConfiguration : IEntityTypeConfiguration<Wallet>
    {
        public void Configure(EntityTypeBuilder<Wallet> builder)
        {
            // Cấu hình độ chính xác tiền tệ: 18 số, 2 số thập phân (Chuẩn kế toán).
            builder.Property(w => w.Balance).HasColumnType("decimal(18,2)");

            // Cấu hình RowVersion cho Optimistic Locking.
            builder.Property(w => w.RowVersion).IsRowVersion();

            // Mỗi User chỉ có 1 Wallet.
            builder.HasIndex(w => w.UserId).IsUnique();
        }
    }
}