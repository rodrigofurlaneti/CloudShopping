using CloudShopping.Infrastructure.Payments;
using Microsoft.EntityFrameworkCore;
namespace CloudShopping.Infrastructure.Persistence;
public sealed partial class AppDbContext
{
    private void ConfigurePayments(ModelBuilder b)
    {
        b.Entity<AsaasConnection>().ToTable("asaasconnections").HasKey(x=>x.Id);
        b.Entity<AsaasConnection>().HasQueryFilter(x=>x.TenantId==_currentTenantId);
        b.Entity<AsaasCustomer>().ToTable("asaascustomers").HasKey(x=>x.Id);
        b.Entity<AsaasCustomer>().HasQueryFilter(x=>x.TenantId==_currentTenantId);
        b.Entity<AsaasCustomer>().HasIndex(x=>new {x.TenantId,x.CustomerId,x.AccountKey}).IsUnique();
        b.Entity<PaymentAttempt>().ToTable("paymentattempts").HasKey(x=>x.Id);
        b.Entity<PaymentAttempt>().HasQueryFilter(x=>x.TenantId==_currentTenantId);
        b.Entity<PaymentAttempt>().HasIndex(x=>x.OrderId).IsUnique();
        b.Entity<PaymentAttempt>().Property(x=>x.Amount).HasColumnType("decimal(12,2)");
        b.Entity<PaymentAttempt>().Property(x=>x.Version).IsConcurrencyToken();
        b.Entity<PaymentOperation>().ToTable("paymentoperations").HasKey(x=>x.Id);
        b.Entity<PaymentOperation>().HasQueryFilter(x=>x.TenantId==_currentTenantId);
        b.Entity<PaymentOperation>().HasIndex(x=>new{x.AttemptId,x.Kind}).IsUnique();
        b.Entity<CloudShopping.Domain.Entities.Orders.Payment>().Property(x=>x.ProviderKey).HasMaxLength(220);
        b.Entity<CloudShopping.Domain.Entities.Orders.Order>().Property(x=>x.FinancialState).HasMaxLength(30);
    }
}
