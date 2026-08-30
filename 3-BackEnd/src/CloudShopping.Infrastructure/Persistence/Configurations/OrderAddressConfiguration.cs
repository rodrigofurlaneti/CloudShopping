using CloudShopping.Domain.Entities.Orders;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CloudShopping.Infrastructure.Persistence.Configurations
{
    public sealed class OrderAddressConfiguration : IEntityTypeConfiguration<OrderAddress>
    {
        public void Configure(EntityTypeBuilder<OrderAddress> builder)
        {
            builder.ToTable("OrderAddresses");
            builder.HasKey(oa => oa.Id);
            builder.Property(oa => oa.OrderId).IsRequired();
            builder.Property(oa => oa.Street).HasMaxLength(200).IsRequired();
            builder.Property(oa => oa.Number).HasMaxLength(20).IsRequired();
            builder.Property(oa => oa.Neighborhood).HasMaxLength(100);
            builder.Property(oa => oa.City).HasMaxLength(100).IsRequired();
            builder.Property(oa => oa.State).HasMaxLength(2).IsRequired();
            builder.Property(oa => oa.ZipCode).HasMaxLength(10).IsRequired();
        }
    }
}
