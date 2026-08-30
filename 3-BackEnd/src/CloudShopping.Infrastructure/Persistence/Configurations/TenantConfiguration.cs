using CloudShopping.Domain.Entities.Tenants;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CloudShopping.Infrastructure.Persistence.Configurations
{
    public sealed class TenantConfiguration : IEntityTypeConfiguration<Tenant>
    {
        public void Configure(EntityTypeBuilder<Tenant> builder)
        {
            builder.ToTable("Tenants");
            builder.HasKey(t => t.Id);
            builder.Property(t => t.CompanyName).HasMaxLength(150).IsRequired();
            builder.Property(t => t.Domain).HasMaxLength(150);
            builder.HasIndex(t => t.Domain).IsUnique();
        }
    }
}
