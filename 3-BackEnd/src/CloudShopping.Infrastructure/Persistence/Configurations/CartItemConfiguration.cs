using CloudShopping.Domain.Entities.Carts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CloudShopping.Infrastructure.Persistence.Configurations
{
    public sealed class CartItemConfiguration : IEntityTypeConfiguration<CartItem>
    {
        public void Configure(EntityTypeBuilder<CartItem> builder)
        {
            builder.ToTable("cartitems");
            builder.HasKey(ci => ci.Id);
            builder.HasIndex(ci => new { ci.CartId, ci.ProductId }).IsUnique();
            builder.Property(ci => ci.CartId)
                .IsRequired();
            builder.Property(ci => ci.ProductId)
                .IsRequired();
            builder.Property(ci => ci.Quantity)
                .IsRequired();
            builder.Property(ci => ci.UnitPrice)
                .HasColumnType("decimal(12,2)")
                .IsRequired();
        }
    }
}
