using System.ComponentModel.DataAnnotations;
using CloudShopping.Api.Security;
using CloudShopping.Application.Features.Access.Commands.ReplaceProfilePermissions;
using CloudShopping.Application.Features.Access.Queries.GetAccessProfiles;
using CloudShopping.Application.Features.Access.Queries.GetAccessChanges;
using MediatR;
using Microsoft.AspNetCore.Mvc;
namespace CloudShopping.Api.Controllers;

[ApiController, Route("api/v1/access")]
public sealed class AccessController(ISender sender) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> List(CancellationToken ct) =>
        CommandResults.Respond(await sender.Send(new GetAccessProfilesQuery(StoreSecurity.Subject(User)), ct), value => Ok(value));

    [HttpPut("profiles/{id:int}")]
    public async Task<IActionResult> Replace(int id, PermissionsInput input, CancellationToken ct) =>
        CommandResults.Respond(await sender.Send(new ReplaceProfilePermissionsCommand(
            StoreSecurity.Subject(User), id, input.Expected, input.Permissions), ct), _ => NoContent());

    [HttpGet("changes")]
    public async Task<IActionResult> Changes(int page = 1, CancellationToken ct = default) =>
        CommandResults.Respond(await sender.Send(new GetAccessChangesQuery(StoreSecurity.Subject(User), page), ct), value => Ok(value));
}
public sealed record PermissionsInput([Required, MaxLength(30)] string[] Expected, [Required, MaxLength(30)] string[] Permissions);
