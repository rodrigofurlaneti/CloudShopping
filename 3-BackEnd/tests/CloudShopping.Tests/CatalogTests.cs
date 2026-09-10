using CloudShopping.Domain.Entities.Products;
using Microsoft.EntityFrameworkCore;
using Xunit;
public sealed partial class CommerceTests
{
 [Fact]
 public async Task Variants_preserve_independent_sku_price_and_stock()
 {
  await using var db=Db(1);var p=await db.Products.SingleAsync();p.ConfigureCatalog("camiseta-azul","Descrição real","Marca",.250m,10,2,15,"shirt","Azul","{\"Cor\":\"Azul\"}");
  var other=Product.Create(1,p.DepartmentId,"SHIRT-RED","Camiseta vermelha",150m,3);other.ConfigureCatalog("camiseta-vermelha","Descrição","Marca",.250m,10,2,15,"shirt","Vermelha","{}");db.Add(other);await db.SaveChangesAsync();
  p.ReserveStock(1);await db.SaveChangesAsync();db.ChangeTracker.Clear();
  var variants=await db.Products.Where(x=>x.FamilyCode=="shirt").ToListAsync();Assert.Equal(2,variants.Count);Assert.Equal(3,variants.Single(x=>x.Sku=="SHIRT-RED").AvailableStock);Assert.Equal(0,variants.Single(x=>x.Id==productId).AvailableStock);
  await using var foreign=Db(2);Assert.Empty(await foreign.Products.Where(x=>x.FamilyCode=="shirt").ToListAsync());
 }
 [Fact]
 public async Task Catalog_rejects_negative_dimensions_and_duplicate_slug()
 {
  await using var db=Db(1);var p=await db.Products.SingleAsync();
  Assert.Throws<ArgumentException>(()=>p.ConfigureCatalog("url-valida","Descrição",null,-1,0,0,0,null,null,"{}"));
  var duplicate=Product.Create(1,p.DepartmentId,"OTHER","Outro",10m,1);duplicate.ConfigureCatalog(p.Slug,"Descrição",null,0,0,0,0,null,null,"{}");db.Add(duplicate);
  await Assert.ThrowsAsync<DbUpdateException>(()=>db.SaveChangesAsync());
 }
}
