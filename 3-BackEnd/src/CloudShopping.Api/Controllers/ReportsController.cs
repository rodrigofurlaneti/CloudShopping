using CloudShopping.Infrastructure.Operations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace CloudShopping.Api.Controllers;
[ApiController,Route("api/v1/reports"),Authorize(Roles="Administrator")]
public sealed class ReportsController(StoreReports reports):ControllerBase
{
 [HttpGet]public Task<object> Summary(DateOnly from,DateOnly to,CancellationToken ct)=>reports.Summary(from,to,ct);
 [HttpGet("export")]public async Task<object> Export(DateOnly from,DateOnly to,CancellationToken ct)=>new{fileName=$"pedidos-{from:yyyy-MM-dd}-{to:yyyy-MM-dd}.csv",content=await reports.Export(from,to,ct)};
}
