using CloudShopping.Domain.Entities.Products;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CloudShopping.Infrastructure.Persistence.Configurations
{
    public sealed class ProductImageConfiguration : IEntityTypeConfiguration<ProductImage>
    {
        public void Configure(EntityTypeBuilder<ProductImage> builder)
        {
            builder.ToTable("ProductImages");
            builder.HasKey(pi => pi.Id);
            builder.Property(pi => pi.ProductId)
                .IsRequired();
            builder.Property(pi => pi.FileName)
                .HasMaxLength(255)
                .IsRequired();
            builder.Property(pi => pi.FilePath)
                .HasMaxLength(500)
                .IsRequired();
            builder.Property(pi => pi.IsPrimary)
                .IsRequired()
                .HasDefaultValue(false);
            builder.Property(pi => pi.DisplayOrder)
                .IsRequired()
                .HasDefaultValue(0);
            builder.HasIndex(pi => pi.ProductId);
        }
    }
}
