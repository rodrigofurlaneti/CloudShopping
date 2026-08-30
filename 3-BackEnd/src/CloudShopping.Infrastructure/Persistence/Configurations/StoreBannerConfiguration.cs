using CloudShopping.Domain.Entities.Store;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CloudShopping.Infrastructure.Persistence.Configurations
{
    public sealed class StoreBannerConfiguration : IEntityTypeConfiguration<StoreBanner>
    {
        public void Configure(EntityTypeBuilder<StoreBanner> builder)
        {
            builder.ToTable("StoreBanners");

            builder.HasKey(b => b.Id);

            builder.Property(b => b.TenantId)
                .IsRequired(false);

            builder.Property(b => b.Title)
                .HasMaxLength(150)
                .IsRequired();

            builder.Property(b => b.Subtitle)
                .HasMaxLength(250);

            builder.Property(b => b.DiscountPercentage)
                .HasMaxLength(10);

            builder.Property(b => b.ButtonText)
                .HasMaxLength(50)
                .IsRequired();

            builder.Property(b => b.ButtonLink)
                .HasMaxLength(250)
                .IsRequired();

            builder.Property(b => b.BackgroundColor)
                .HasMaxLength(30)
                .IsRequired();

            builder.HasIndex(b => new { b.TenantId, b.DisplayOrder });
        }
    }
}