using IAMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IAMS.Persistence.Configurations
{
    public class MarketerConfiguration : IEntityTypeConfiguration<Marketer>
    {
        public void Configure(EntityTypeBuilder<Marketer> builder)
        {
            builder.ToTable("Marketers");

            builder.HasKey(m => m.Id);

            builder.Property(m => m.Code)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(m => m.Name)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(m => m.ContactPerson)
                .HasMaxLength(200);

            builder.Property(m => m.PhoneNumber)
                .HasMaxLength(20);

            builder.Property(m => m.Email)
                .HasMaxLength(100);

            builder.Property(m => m.Address)
                .HasMaxLength(500);

            builder.Property(m => m.CommissionRate)
                .HasColumnType("decimal(5,2)");

            builder.Property(m => m.Notes)
                .HasMaxLength(1000);

            builder.Property(m => m.IsActive)
                .IsRequired()
                .HasDefaultValue(true);

            // Indexes
            builder.HasIndex(m => m.Code)
                .IsUnique()
                .HasDatabaseName("IX_Marketers_Code");

            builder.HasIndex(m => m.Name)
                .HasDatabaseName("IX_Marketers_Name");

            builder.HasIndex(m => m.IsActive)
                .HasDatabaseName("IX_Marketers_IsActive");

            // Soft delete filter
            builder.HasQueryFilter(m => !m.IsDeleted);

            // Relationships
            builder.HasMany(m => m.Policies)
                .WithOne(p => p.Marketer)
                .HasForeignKey(p => p.MarketerId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
