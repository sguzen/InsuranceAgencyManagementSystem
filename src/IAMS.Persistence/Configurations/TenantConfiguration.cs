using IAMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IAMS.Persistence.Configurations
{
    public class TenantConfiguration : IEntityTypeConfiguration<Tenant>
    {
        public void Configure(EntityTypeBuilder<Tenant> builder)
        {
            builder.HasKey(t => t.Id);

            builder.Property(t => t.Name)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(t => t.Identifier)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(t => t.ExternalId)
                .HasMaxLength(20); // Agency number for policy display

            builder.Property(t => t.ConnectionString)
                .HasMaxLength(1000);

            builder.HasIndex(t => t.Identifier)
                .IsUnique();
        }
    }
}