using CloudShopping.Domain.Entities.Products;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CloudShopping.Infrastructure.Persistence.Configurations
{
    public sealed class DepartmentConfiguration : IEntityTypeConfiguration<Department>
    {
        public void Configure(EntityTypeBuilder<Department> builder)
        {
            builder.ToTable("Departments");

            builder.HasKey(d => d.Id);

            // O TenantId não é Required pois pode ser nulo (padrões globais do sistema)
            builder.Property(d => d.TenantId)
                .IsRequired(false);

            builder.Property(d => d.Name)
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(d => d.Slug)
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(d => d.IsSystemDefault)
                .IsRequired()
                .HasDefaultValue(false);

            // Garantir que Nome e Slug sejam únicos por Tenant
            builder.HasIndex(d => new { d.TenantId, d.Name })
                .IsUnique();

            builder.HasIndex(d => new { d.TenantId, d.Slug })
                .IsUnique();
        }
    }
}