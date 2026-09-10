using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using CloudShopping.Domain.Entities.Products;
using CloudShopping.Infrastructure.Persistence;
using CloudShopping.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace CloudShopping.Api.Controllers;
[ApiController,Route("api/v1/catalog-details"),Authorize(Roles="Administrator")]
public sealed class CatalogDetailsController(AppDbContext db):ControllerBase
{
 [HttpGet]public async Task<object> List(int page=1,CancellationToken ct=default)=>await db.Products.AsNoTracking().OrderBy(x=>x.Name).Skip((page-1)*20).Take(20).Select(x=>new{x.Id,x.Name,x.Sku,x.Version,x.Slug,x.Description,x.Brand,x.WeightKg,x.WidthCm,x.HeightCm,x.LengthCm,x.FamilyCode,x.VariantLabel,x.AttributesJson}).ToListAsync(ct);
 [HttpPut("{id:int}")]public async Task<IActionResult> Update(int id,CatalogDetailsInput input,CancellationToken ct)
 {
  var p=await db.Products.SingleOrDefaultAsync(x=>x.Id==id,ct)??throw new KeyNotFoundException();
  if(p.Version!=input.Version)throw new CommerceConflictException("Produto atualizado. Recarregue a ficha antes de salvar.");
  if(input.Attributes==null||input.Attributes.Count>12||input.Attributes.Any(x=>string.IsNullOrWhiteSpace(x.Key)||x.Key.Length>80||string.IsNullOrWhiteSpace(x.Value)||x.Value.Length>150))throw new ArgumentException("Informe até 12 atributos, com nome e valor curtos.");
  var family=string.IsNullOrWhiteSpace(input.FamilyCode)?null:input.FamilyCode.Trim();var variant=string.IsNullOrWhiteSpace(input.VariantLabel)?null:input.VariantLabel.Trim();
  if(await db.Products.IgnoreQueryFilters().AnyAsync(x=>x.TenantId==db.CurrentTenantId&&x.Id!=id&&(x.Slug==input.Slug||(family!=null&&x.FamilyCode==family&&x.VariantLabel==variant)),ct))throw new CommerceConflictException("URL ou variante já utilizada nesta loja.");
  p.ConfigureCatalog(input.Slug,input.Description,input.Brand,input.WeightKg,input.WidthCm,input.HeightCm,input.LengthCm,family,variant,JsonSerializer.Serialize(input.Attributes));
  try{await db.SaveChangesAsync(ct);}catch(DbUpdateException){throw new CommerceConflictException("Produto ou URL alterado por outra operação. Recarregue.");}
  return NoContent();
 }
}
public sealed record CatalogDetailsInput(int Version,[Required,MaxLength(150)]string Slug,[MaxLength(10000)]string Description,[MaxLength(100)]string? Brand,decimal WeightKg,decimal WidthCm,decimal HeightCm,decimal LengthCm,[MaxLength(50)]string? FamilyCode,[MaxLength(100)]string? VariantLabel,Dictionary<string,string> Attributes);
