using CloudShopping.Domain.Entities.Backoffice;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CloudShopping.Infrastructure.Persistence.Configurations
{
    public sealed class EmployeeUserConfiguration : IEntityTypeConfiguration<EmployeeUser>
    {
        public void Configure(EntityTypeBuilder<EmployeeUser> builder)
        {
            builder.ToTable("EmployeeUsers");
            builder.HasKey(u => u.Id);

            builder.Property(u => u.TenantId)
                .IsRequired();

            builder.Property(u => u.EmployeeId)
                .IsRequired();

            builder.Property(u => u.Username)
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(u => u.PasswordHash)
                .HasMaxLength(255)
                .IsRequired();

            builder.HasOne<Employee>()
                .WithMany()
                .HasForeignKey(u => u.EmployeeId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(u => new { u.TenantId, u.Username })
                .IsUnique();
        }
    }
}