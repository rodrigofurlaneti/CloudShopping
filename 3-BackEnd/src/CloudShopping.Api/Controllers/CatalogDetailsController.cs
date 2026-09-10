using CloudShopping.Application.Features.Products.Queries.GetCatalogDetails;
using CloudShopping.Application.Features.Products.Commands.UpdateCatalogDetails;
using System.ComponentModel.DataAnnotations;
using CloudShopping.Application.Features.Products.Commands;
using CloudShopping.Application.Features.Products.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace CloudShopping.Api.Controllers;
[ApiController, Route("api/v1/catalog-details"), Authorize(Roles = "Administrator")]
public sealed class CatalogDetailsController(ISender sender) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> List(int page = 1, CancellationToken ct = default) => CommandResults.Respond(await sender.Send(new GetCatalogDetailsQuery(page), ct), items => Ok(items));
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, CatalogDetailsInput input, CancellationToken ct)
    {
        return CommandResults.Respond(await sender.Send(new UpdateCatalogDetailsCommand(id, input.Version, input.Slug, input.Description, input.Brand,
            input.WeightKg, input.WidthCm, input.HeightCm, input.LengthCm, input.FamilyCode, input.VariantLabel, input.Attributes), ct), _ => NoContent());
    }
}
public sealed record CatalogDetailsInput(int Version,[Required,MaxLength(150)]string Slug,[MaxLength(10000)]string Description,[MaxLength(100)]string? Brand,decimal WeightKg,decimal WidthCm,decimal HeightCm,decimal LengthCm,[MaxLength(50)]string? FamilyCode,[MaxLength(100)]string? VariantLabel,Dictionary<string,string> Attributes);
