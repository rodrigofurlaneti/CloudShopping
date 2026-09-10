using CloudShopping.Application.Features.Reports.Queries.GetReportSummary;
using CloudShopping.Application.Features.Reports.Queries.ExportReport;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace CloudShopping.Api.Controllers;
[ApiController, Route("api/v1/reports"), Authorize(Roles = "Administrator")]
public sealed class ReportsController(ISender sender) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Summary(DateOnly from, DateOnly to, CancellationToken ct) =>
        CommandResults.Respond(await sender.Send(new GetReportSummaryQuery(from, to), ct), value => Ok(value));
    [HttpGet("export")]
    public async Task<IActionResult> Export(DateOnly from, DateOnly to, CancellationToken ct) =>
        CommandResults.Respond(await sender.Send(new ExportReportQuery(from, to), ct), value => Ok(value));
}
