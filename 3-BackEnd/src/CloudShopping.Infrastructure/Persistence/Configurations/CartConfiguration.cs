using CloudShopping.Domain.Entities.Carts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudShopping.Infrastructure.Persistence.Configurations
{
    public sealed class CartConfiguration : IEntityTypeConfiguration<Cart>
    {
        public void Configure(EntityTypeBuilder<Cart> builder)
        {
            builder.ToTable("Carts");
            builder.HasKey(c => c.Id);
            builder.Property(c => c.CustomerId)
                .IsRequired();
            builder.HasIndex(c => c.CustomerId)
                .IsUnique();
            builder.Property(c => c.ExpiresAt)
                .ValueGeneratedOnAddOrUpdate()
                .Metadata.SetBeforeSaveBehavior(PropertySaveBehavior.Ignore);
            builder.HasMany(c => c.Items)
                .WithOne()
                .HasForeignKey(ci => ci.CartId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
