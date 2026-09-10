using CloudShopping.Application.Features.Coupons.Commands.CreateCoupon;
using CloudShopping.Application.Features.Coupons.Commands.SetCouponEnabled;
using CloudShopping.Application.Features.Coupons.Queries.GetCoupons;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace CloudShopping.Api.Controllers;

[ApiController, Route("api/v1/coupons"), Authorize(Roles = "Administrator")]
public sealed class CouponsController(ISender sender) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> List(int page = 1, CancellationToken ct = default) =>
        Ok(await sender.Send(new GetCouponsQuery(page), ct));

    [HttpPost]
    public async Task<IActionResult> Create(CreateCouponCommand input, CancellationToken ct) =>
        CommandResults.Respond(await sender.Send(input, ct), result => Ok(result));

    [HttpPut("{id}/enabled")]
    public async Task<IActionResult> Enable(string id, EnableCouponInput input, CancellationToken ct) =>
        CommandResults.Respond(await sender.Send(new SetCouponEnabledCommand(id, input.Version, input.Enabled), ct), _ => NoContent());
}
public sealed record EnableCouponInput(int Version, bool Enabled);
