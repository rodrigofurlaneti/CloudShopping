using CloudShopping.Domain.Entities.Carts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudShopping.Infrastructure.Persistence.Configurations
{
    public sealed class CartItemConfiguration : IEntityTypeConfiguration<CartItem>
    {
        public void Configure(EntityTypeBuilder<CartItem> builder)
        {
            builder.ToTable("CartItems");
            builder.HasKey(ci => ci.Id);
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
