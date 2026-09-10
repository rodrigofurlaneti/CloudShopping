using System.ComponentModel.DataAnnotations;
using CloudShopping.Api.Security;
using CloudShopping.Infrastructure.Operations;
using CloudShopping.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace CloudShopping.Api.Controllers;
[ApiController,Route("api/v1/catalog-imports"),Authorize(Roles="Administrator")]
public sealed class CatalogImportsController(AppDbContext db):ControllerBase
{
 [HttpGet]public Task<object> List(int page=1,CancellationToken ct=default)=>new CatalogImportService(db).List(page,ct);
 [HttpGet("{id}")]public Task<object> Detail(string id,CancellationToken ct)=>new CatalogImportService(db).Detail(id,ct);
 [HttpPost]public Task<object> Preview(ImportInput input,CancellationToken ct)=>new CatalogImportService(db).Preview(input.Csv,"Administrator:"+StoreSecurity.Subject(User),ct);
 [HttpPost("{id}/queue")]public async Task<IActionResult> Queue(string id,CancellationToken ct){await new CatalogImportService(db).Queue(id,ct);return NoContent();}
}
public sealed record ImportInput([Required,MaxLength(200000)]string Csv);
