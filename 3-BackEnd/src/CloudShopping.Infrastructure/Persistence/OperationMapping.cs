using CloudShopping.Infrastructure.Operations;
using Microsoft.EntityFrameworkCore;
namespace CloudShopping.Infrastructure.Persistence;
public sealed partial class AppDbContext
{
    private void ConfigureOperations(ModelBuilder b)
    {
        b.Entity<OperationEvent>().ToTable("operationevents").HasKey(x=>x.Id);
        b.Entity<OperationEvent>().HasQueryFilter(x=>x.TenantId==_currentTenantId);
        b.Entity<OperationEvent>().HasIndex(x=>new{x.TenantId,x.OrderId,x.OperationKey}).IsUnique();
        b.Entity<Shipment>().ToTable("shipments").HasKey(x=>x.Id);
        b.Entity<Shipment>().HasQueryFilter(x=>x.TenantId==_currentTenantId);
        b.Entity<ShipmentItem>().ToTable("shipmentitems").HasKey(x=>x.Id);
        b.Entity<ShipmentItem>().HasQueryFilter(x=>x.TenantId==_currentTenantId);
        b.Entity<ShipmentItem>().HasOne<Shipment>().WithMany().HasForeignKey(x=>x.ShipmentId).OnDelete(DeleteBehavior.Restrict);
        b.Entity<ReturnCase>().ToTable("returncases").HasKey(x=>x.Id);
        b.Entity<ReturnCase>().HasQueryFilter(x=>x.TenantId==_currentTenantId);
        b.Entity<ReturnItem>().ToTable("returnitems").HasKey(x=>x.Id);
        b.Entity<ReturnItem>().HasQueryFilter(x=>x.TenantId==_currentTenantId);
        b.Entity<ReturnItem>().HasOne<ReturnCase>().WithMany().HasForeignKey(x=>x.ReturnCaseId).OnDelete(DeleteBehavior.Restrict);
    }
}
