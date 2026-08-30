using CloudShopping.Domain.Entities.Backoffice;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CloudShopping.Infrastructure.Persistence.Configurations
{
    public sealed class EmployeeConfiguration : IEntityTypeConfiguration<Employee>
    {
        public void Configure(EntityTypeBuilder<Employee> builder)
        {
            builder.ToTable("Employees");
            builder.HasKey(e => e.Id);

            builder.Property(e => e.TenantId)
                .IsRequired();

            builder.Property(e => e.Name)
                .HasMaxLength(150)
                .IsRequired();

            builder.Property(e => e.Cpf)
                .HasMaxLength(11)
                .IsRequired();

            builder.Property(e => e.Email)
                .HasMaxLength(150);

            builder.Property(e => e.Phone)
                .HasMaxLength(20);

            builder.Property(e => e.Salary)
                .HasColumnType("decimal(12,2)");

            builder.Property(e => e.CommissionPercent)
                .HasColumnType("decimal(5,2)");

            builder.HasIndex(e => new { e.TenantId, e.Cpf })
                .IsUnique();
        }
    }
}