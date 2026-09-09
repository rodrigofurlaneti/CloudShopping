using CloudShopping.Domain.Entities.Customers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CloudShopping.Infrastructure.Persistence.Configurations
{
    public sealed class CustomerAddressConfiguration : IEntityTypeConfiguration<Address>
    {
        public void Configure(EntityTypeBuilder<Address> builder)
        {
            builder.ToTable("addresses");
            builder.HasKey(a => a.Id);
            builder.Property(a => a.CustomerId).IsRequired();
            builder.Property(a => a.Street).HasMaxLength(150).IsRequired();
            builder.Property(a => a.Number).HasMaxLength(10).IsRequired();
            builder.Property(a => a.Neighborhood).HasMaxLength(50);
            builder.Property(a => a.City).HasMaxLength(50).IsRequired();
            builder.Property(a => a.State).HasMaxLength(2).IsRequired();
            builder.Property(a => a.ZipCode).HasMaxLength(8).IsRequired();
        }
    }
}
