using CloudShopping.Domain.Entities.Orders;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CloudShopping.Infrastructure.Persistence.Configurations
{
    public sealed class OrderStateHistoryConfiguration : IEntityTypeConfiguration<OrderStateHistory>
    {
        public void Configure(EntityTypeBuilder<OrderStateHistory> builder)
        {
            builder.ToTable("OrderStateHistories");
            builder.HasKey(h => h.Id);
            builder.Property(h => h.OrderId).IsRequired();
            builder.Property(h => h.OrderStatusId).IsRequired();
            builder.Property(h => h.Notes).HasMaxLength(255);
            builder.Property(h => h.IsActive).HasDefaultValue(true);
            builder.Property(h => h.CreatedAt).IsRequired();
            builder.Property(h => h.UpdatedAt).IsRequired();
            builder.HasIndex(h => h.OrderId);
        }
    }
}
