using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TicketBooking.Domain.Entities;

namespace TicketBooking.Infrastructure.Data.Configurations
{
    public class EventConfiguration : IEntityTypeConfiguration<Event>
    {
        public void Configure(EntityTypeBuilder<Event> builder)
        {
            // --- 1. CẤU HÌNH CƠ BẢN (Từ Day 2) ---
            builder.HasKey(e => e.Id);

            builder.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(200);

            // Cấu hình quan hệ: 1 Venue có nhiều Events
            builder.HasOne(e => e.Venue)
                .WithMany(v => v.Events)
                .HasForeignKey(e => e.VenueId)
                .OnDelete(DeleteBehavior.Restrict);

            // --- 2. CẤU HÌNH HIỆU NĂNG (DAY 10: INDEXING) ---
            // Phần này giúp tính năng Tìm kiếm chạy siêu nhanh

            // Index cho tên sự kiện (Tìm kiếm theo từ khóa)
            builder.HasIndex(e => e.Name)
                   .HasDatabaseName("IX_Events_Name");

            // Index cho ngày bắt đầu (Tìm kiếm theo khoảng thời gian)
            builder.HasIndex(e => e.StartDateTime)
                   .HasDatabaseName("IX_Events_StartDateTime");

            // (Optional) Index kết hợp Venue + Ngày (nếu hay lọc theo địa điểm)
            builder.HasIndex(e => new { e.VenueId, e.StartDateTime })
                   .HasDatabaseName("IX_Events_Venue_Date");
        }
    }
}