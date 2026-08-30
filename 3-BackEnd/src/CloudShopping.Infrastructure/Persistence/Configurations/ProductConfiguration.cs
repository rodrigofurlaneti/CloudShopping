using CloudShopping.Domain.Entities.Products;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CloudShopping.Infrastructure.Persistence.Configurations
{
    public sealed class ProductConfiguration : IEntityTypeConfiguration<Product>
    {
        public void Configure(EntityTypeBuilder<Product> builder)
        {
            builder.ToTable("Products");
            builder.HasKey(p => p.Id);
            builder.Property(p => p.TenantId)
                .IsRequired();
            builder.Property(p => p.Sku)
                .HasMaxLength(30)
                .IsRequired();
            builder.Property(p => p.Name)
                .HasMaxLength(150)
                .IsRequired();
            builder.Property(p => p.Price)
                .HasColumnType("decimal(12,2)")
                .IsRequired();
            builder.Property(p => p.PhysicalStock)
                .IsRequired();
            builder.Property(p => p.ReservedStock)
                .IsRequired();
            builder.Property(p => p.AvailableStock)
                .HasComputedColumnSql("PhysicalStock - ReservedStock", stored: false);
            builder.Property(p => p.Version)
                .IsRequired()
                .HasDefaultValue(1);
            builder.OwnsOne(p => p.Location, location =>
            {
                location.Property(l => l.Aisle).HasColumnName("Location_Aisle").HasMaxLength(10);
                location.Property(l => l.Rack).HasColumnName("Location_Rack").HasMaxLength(10);
                location.Property(l => l.Level).HasColumnName("Location_Level").HasMaxLength(10);
                location.Property(l => l.Position).HasColumnName("Location_Position").HasMaxLength(10);
            });
            builder.HasMany(p => p.Images)
                .WithOne()
                .HasForeignKey(i => i.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
            builder.HasIndex(p => new { p.TenantId, p.Sku })
                .IsUnique();
        }
    }
}
