using CloudShopping.Domain.Entities.Customers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace CloudShopping.Infrastructure.Persistence.Configurations
{
    public sealed class CustomerConfiguration : IEntityTypeConfiguration<Customer>
    {
        public void Configure(EntityTypeBuilder<Customer> builder)
        {
            builder.ToTable("Customers");
            builder.HasKey(c => c.Id);
            builder.Property(c => c.TenantId)
                .IsRequired();
            builder.Property(c => c.Email)
                .HasMaxLength(256);
            builder.Property(c => c.PasswordHash)
                .HasMaxLength(512);
            builder.HasIndex(c => new { c.TenantId, c.Email })
                .IsUnique()
                .HasFilter("[Email] IS NOT NULL");
            builder.HasOne(c => c.Individual)
                .WithOne()
                .HasForeignKey<Individual>(i => i.CustomerId)
                .OnDelete(DeleteBehavior.Cascade);
            builder.HasOne(c => c.Company)
                .WithOne()
                .HasForeignKey<Company>(cp => cp.CustomerId)
                .OnDelete(DeleteBehavior.Cascade);
            builder.HasMany(c => c.Addresses)
                .WithOne()
                .HasForeignKey(a => a.CustomerId)
                .OnDelete(DeleteBehavior.Cascade);
            builder.HasMany(c => c.Contacts)
                .WithOne()
                .HasForeignKey(ct => ct.CustomerId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
