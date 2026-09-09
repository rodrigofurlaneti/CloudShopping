using CloudShopping.Domain.Entities.Tenants;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CloudShopping.Infrastructure.Persistence.Configurations
{
    public sealed class TenantConfiguration : IEntityTypeConfiguration<Tenant>
    {
        public void Configure(EntityTypeBuilder<Tenant> builder)
        {
            builder.ToTable("tenants");
            builder.HasKey(t => t.Id);
            builder.Property(t => t.CompanyName).HasMaxLength(100).IsRequired();
            builder.Property(t => t.Domain).HasMaxLength(100);
            builder.HasIndex(t => t.Domain).IsUnique();
        }
    }
}
