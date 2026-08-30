using CloudShopping.Domain.Entities.Backoffice;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CloudShopping.Infrastructure.Persistence.Configurations
{
    public sealed class ProfileUserConfiguration : IEntityTypeConfiguration<ProfileUser>
    {
        public void Configure(EntityTypeBuilder<ProfileUser> builder)
        {
            builder.ToTable("ProfileUsers");
            builder.HasKey(pu => pu.Id);

            builder.Property(pu => pu.TenantId)
                .IsRequired();

            builder.Property(pu => pu.ProfileId)
                .IsRequired();

            builder.Property(pu => pu.EmployeeUserId)
                .IsRequired();

            builder.HasOne<Profile>()
                .WithMany()
                .HasForeignKey(pu => pu.ProfileId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne<EmployeeUser>()
                .WithMany()
                .HasForeignKey(pu => pu.EmployeeUserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(pu => new { pu.ProfileId, pu.EmployeeUserId })
                .IsUnique();
        }
    }
}