using CloudShopping.Domain.Entities.Customers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CloudShopping.Infrastructure.Persistence.Configurations
{
    public sealed class CompanyConfiguration : IEntityTypeConfiguration<Company>
    {
        public void Configure(EntityTypeBuilder<Company> builder)
        {
            builder.ToTable("companies");
            builder.HasKey(c => c.Id);
            builder.Property(c => c.Id).HasColumnName("CustomerId").ValueGeneratedNever();
            builder.Property(c => c.BusinessTaxId).HasMaxLength(14).IsRequired();
            builder.Property(c => c.CompanyName).HasMaxLength(150).IsRequired();
            builder.Property(c => c.StateTaxId).HasMaxLength(15);
        }
    }
}
