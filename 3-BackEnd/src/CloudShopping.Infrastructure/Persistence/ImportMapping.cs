using CloudShopping.Infrastructure.Operations;
using Microsoft.EntityFrameworkCore;
namespace CloudShopping.Infrastructure.Persistence;
public sealed partial class AppDbContext
{
 private void ConfigureImports(ModelBuilder b)
 {
  b.Entity<CatalogImport>().ToTable("catalogimports").HasKey(x=>x.Id);
  b.Entity<CatalogImport>().HasQueryFilter(x=>x.TenantId==_currentTenantId);
  b.Entity<CatalogImportRow>().ToTable("catalogimportrows").HasKey(x=>x.Id);
  b.Entity<CatalogImportRow>().HasQueryFilter(x=>x.TenantId==_currentTenantId);
  b.Entity<CatalogImportRow>().HasOne<CatalogImport>().WithMany().HasForeignKey(x=>x.ImportId).OnDelete(DeleteBehavior.Restrict);
  b.Entity<CatalogImportRow>().Property(x=>x.Price).HasColumnType("decimal(12,2)");
  b.Entity<CloudShopping.Domain.Entities.Products.Product>().Property(x=>x.WeightKg).HasColumnType("decimal(10,3)");
  b.Entity<CloudShopping.Domain.Entities.Products.Product>().Property(x=>x.WidthCm).HasColumnType("decimal(10,2)");
  b.Entity<CloudShopping.Domain.Entities.Products.Product>().Property(x=>x.HeightCm).HasColumnType("decimal(10,2)");
  b.Entity<CloudShopping.Domain.Entities.Products.Product>().Property(x=>x.LengthCm).HasColumnType("decimal(10,2)");
 }
}
