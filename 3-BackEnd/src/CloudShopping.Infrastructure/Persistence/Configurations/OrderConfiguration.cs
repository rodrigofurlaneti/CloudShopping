using CloudShopping.Domain.Entities.Orders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudShopping.Infrastructure.Persistence.Configurations
{
    public sealed class OrderConfiguration : IEntityTypeConfiguration<Order>
    {
        public void Configure(EntityTypeBuilder<Order> builder)
        {
            builder.ToTable("Orders");
            builder.HasKey(o => o.Id);
            builder.Property(o => o.TenantId)
                .IsRequired();
            builder.Property(o => o.TotalAmount)
                .HasColumnType("decimal(18,2)")
                .IsRequired();
            builder.Property(o => o.DiscountAmount)
                .HasColumnType("decimal(18,2)");
            builder.Property(o => o.ShippingAmount)
                .HasColumnType("decimal(18,2)");
            builder.HasMany(o => o.OrderItems)
                .WithOne()
                .HasForeignKey(oi => oi.OrderId)
                .OnDelete(DeleteBehavior.Cascade);
            builder.HasMany(o => o.Payments)
                .WithOne()
                .HasForeignKey(p => p.OrderId)
                .OnDelete(DeleteBehavior.Cascade);
            builder.HasOne(o => o.OrderAddress)
                .WithOne()
                .HasForeignKey<OrderAddress>(oa => oa.OrderId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
