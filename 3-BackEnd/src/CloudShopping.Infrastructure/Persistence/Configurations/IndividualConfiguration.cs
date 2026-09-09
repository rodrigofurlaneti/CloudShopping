using CloudShopping.Domain.Entities.Customers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CloudShopping.Infrastructure.Persistence.Configurations
{
    public sealed class IndividualConfiguration : IEntityTypeConfiguration<Individual>
    {
        public void Configure(EntityTypeBuilder<Individual> builder)
        {
            builder.ToTable("individuals");
            builder.HasKey(i => i.Id);
            builder.Property(i => i.Id).HasColumnName("CustomerId").ValueGeneratedNever();
            builder.Property(i => i.TaxId).HasMaxLength(11).IsRequired();
            builder.Property(i => i.FullName).HasMaxLength(150).IsRequired();
        }
    }
}
