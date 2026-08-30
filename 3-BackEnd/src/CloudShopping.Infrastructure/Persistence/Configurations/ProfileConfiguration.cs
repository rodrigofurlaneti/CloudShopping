using CloudShopping.Domain.Entities.Backoffice;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CloudShopping.Infrastructure.Persistence.Configurations
{
    public sealed class ProfileConfiguration : IEntityTypeConfiguration<Profile>
    {
        public void Configure(EntityTypeBuilder<Profile> builder)
        {
            builder.ToTable("Profiles");
            builder.HasKey(p => p.Id);

            builder.Property(p => p.TenantId)
                .IsRequired();

            builder.Property(p => p.Name)
                .HasMaxLength(100)
                .IsRequired();

            builder.HasIndex(p => new { p.TenantId, p.Name })
                .IsUnique();
        }
    }
}