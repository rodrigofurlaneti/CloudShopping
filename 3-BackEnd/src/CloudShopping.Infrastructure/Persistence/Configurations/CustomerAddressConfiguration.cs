using CloudShopping.Domain.Entities.Customers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CloudShopping.Infrastructure.Persistence.Configurations
{
    public sealed class CustomerAddressConfiguration : IEntityTypeConfiguration<Address>
    {
        public void Configure(EntityTypeBuilder<Address> builder)
        {
            builder.ToTable("CustomerAddresses");
            builder.HasKey(a => a.Id);
            builder.Property(a => a.CustomerId).IsRequired();
            builder.Property(a => a.Street).HasMaxLength(200).IsRequired();
            builder.Property(a => a.Number).HasMaxLength(20).IsRequired();
            builder.Property(a => a.Neighborhood).HasMaxLength(100);
            builder.Property(a => a.City).HasMaxLength(100).IsRequired();
            builder.Property(a => a.State).HasMaxLength(2).IsRequired();
            builder.Property(a => a.ZipCode).HasMaxLength(10).IsRequired();
        }
    }
}
