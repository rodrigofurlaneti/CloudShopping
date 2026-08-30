using CloudShopping.Domain.Entities.Orders;
using CloudShopping.Domain.Entities.Tenants;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CloudShopping.Infrastructure.Persistence.Configurations
{
    public sealed class OrderSectorConfiguration : IEntityTypeConfiguration<OrderSector>
    {
        public void Configure(EntityTypeBuilder<OrderSector> builder)
        {
            builder.ToTable("OrderSectors");
            builder.HasKey(os => os.Id);
            builder.Property(os => os.Id)
                .ValueGeneratedOnAdd();
            builder.Property(os => os.Name)
                .IsRequired()
                .HasMaxLength(100);
            builder.Property(os => os.IsActive)
                .HasDefaultValue(true);
            builder.Property(os => os.CreatedAt)
                .HasColumnType("datetime(6)")
                .HasDefaultValueSql("CURRENT_TIMESTAMP(6)");
            builder.Property(os => os.UpdatedAt)
                .HasColumnType("datetime(6)")
                .HasDefaultValueSql("CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6)");
            builder.HasIndex(os => new { os.TenantId, os.Name })
                .IsUnique()
                .HasDatabaseName("uk_tenant_sector_name");
            builder.HasOne<Tenant>()
                .WithMany()
                .HasForeignKey(os => os.TenantId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
