using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudShopping.Infrastructure.Persistence.Configurations
{
    public sealed class OrderStatusConfiguration : IEntityTypeConfiguration<OrderStatus>
    {
        public void Configure(EntityTypeBuilder<OrderStatus> builder)
        {
            builder.ToTable("OrderStatus");
            builder.HasKey(os => os.Id);
            builder.Property(os => os.Id)
                .ValueGeneratedOnAdd();
            builder.Property(os => os.TenantId)
                .IsRequired();
            builder.Property(os => os.OrderSectorId)
                .IsRequired();
            builder.Property(os => os.Name)
                .IsRequired()
                .HasMaxLength(50);
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
                .HasDatabaseName("uk_tenant_status_name");
            builder.HasOne<Domain.Entities.Tenants.Tenant>()
                .WithMany()
                .HasForeignKey(os => os.TenantId)
                .OnDelete(DeleteBehavior.Restrict);
            builder.HasOne<OrderSector>()
                .WithMany()
                .HasForeignKey(os => os.OrderSectorId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
