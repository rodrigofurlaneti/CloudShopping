using CloudShopping.Domain.Entities.Products;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CloudShopping.Infrastructure.Persistence.Configurations
{
    public sealed class StockMovementConfiguration : IEntityTypeConfiguration<StockMovement>
    {
        public void Configure(EntityTypeBuilder<StockMovement> builder)
        {
            builder.ToTable("StockMovements");
            builder.HasKey(sm => sm.Id);
            builder.Property(sm => sm.ProductId)
                .IsRequired();
            builder.Property(sm => sm.MovementType)
                .HasConversion<string>()
                .HasMaxLength(30)
                .IsRequired();
            builder.Property(sm => sm.QuantityChanged)
                .IsRequired();
            builder.Property(sm => sm.BalanceAfterMovement)
                .IsRequired();
            builder.Property(sm => sm.Reason)
                .HasMaxLength(150)
                .IsRequired();
            builder.Property(sm => sm.CreatedAt)
                .IsRequired();
            builder.HasIndex(sm => sm.ProductId);
        }
    }
}
