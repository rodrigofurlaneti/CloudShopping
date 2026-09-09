using CloudShopping.Domain.Entities.Orders;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CloudShopping.Infrastructure.Persistence.Configurations
{
    public sealed class OrderAddressConfiguration : IEntityTypeConfiguration<OrderAddress>
    {
        public void Configure(EntityTypeBuilder<OrderAddress> builder)
        {
            builder.ToTable("orderaddresses");
            builder.Ignore(oa => oa.Id);
            builder.HasKey(oa => oa.OrderId);
            builder.Property(oa => oa.OrderId).ValueGeneratedNever();
            builder.Property(oa => oa.OrderId).IsRequired();
            builder.Property(oa => oa.Street).HasMaxLength(150).IsRequired();
            builder.Property(oa => oa.Number).HasMaxLength(10).IsRequired();
            builder.Property(oa => oa.Neighborhood).HasMaxLength(50);
            builder.Property(oa => oa.City).HasMaxLength(50).IsRequired();
            builder.Property(oa => oa.State).HasMaxLength(2).IsRequired();
            builder.Property(oa => oa.ZipCode).HasMaxLength(8).IsRequired();
        }
    }
}
