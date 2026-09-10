using CloudShopping.Infrastructure.Operations;
using CloudShopping.Domain.Entities.Orders;
using Microsoft.EntityFrameworkCore;
namespace CloudShopping.Infrastructure.Persistence;
public sealed partial class AppDbContext
{
 private void ConfigureOutbox(ModelBuilder b)
 {
  b.Entity<CommerceOutbox>().ToTable("commerceoutbox").HasKey(x=>x.Id);
  b.Entity<CommerceOutbox>().HasQueryFilter(x=>x.TenantId==_currentTenantId);
  b.Entity<CommerceOutbox>().HasOne(x=>x.Order).WithMany().HasForeignKey(x=>x.OrderId).OnDelete(DeleteBehavior.Restrict);
  b.Entity<CommerceOutbox>().HasIndex(x=>new{x.OrderId,x.OrderVersion,x.Kind}).IsUnique();
  b.Entity<CustomerNotification>().ToTable("customernotifications").HasKey(x=>x.Id);
  b.Entity<CustomerNotification>().HasQueryFilter(x=>x.TenantId==_currentTenantId);
 }
 private void CaptureCommerceEvents()
 {
  foreach(var entry in ChangeTracker.Entries<Order>().Where(x=>x.State is EntityState.Added or EntityState.Modified).ToList())
  {
   var o=entry.Entity;var kinds=new List<string>();
   if(entry.State==EntityState.Added)kinds.Add("OrderCreated");
   else
   {
    if(entry.Property(x=>x.FinancialState).IsModified)kinds.Add("Payment"+o.FinancialState);
    if(entry.Property(x=>x.FulfillmentState).IsModified)kinds.Add("Fulfillment"+o.FulfillmentState);
    if(entry.Property(x=>x.ReservationState).IsModified&&o.ReservationState=="Released")kinds.Add("OrderCancelled");
   }
   foreach(var kind in kinds)
    if(!Set<CommerceOutbox>().Local.Any(x=>x.Order==o&&x.OrderVersion==o.Version&&x.Kind==kind))
     Add(new CommerceOutbox {TenantId=o.TenantId,Order=o,OrderVersion=o.Version,Kind=kind});
  }
 }
}
