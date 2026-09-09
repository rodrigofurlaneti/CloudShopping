using CloudShopping.Domain.Entities.Orders;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CloudShopping.Infrastructure.Persistence.Configurations
{
    public sealed class PaymentConfiguration : IEntityTypeConfiguration<Payment>
    {
        public void Configure(EntityTypeBuilder<Payment> builder)
        {
            builder.ToTable("payments");
            builder.HasKey(p => p.Id);
            builder.Property(p => p.OrderId).IsRequired();
            builder.Property(p => p.PaymentMethod).HasMaxLength(50).IsRequired();
            builder.Property(p => p.Amount).HasColumnType("decimal(12,2)").IsRequired();
            builder.Property(p => p.PaymentStatusId).HasConversion<int>().IsRequired();
        }
    }
}
