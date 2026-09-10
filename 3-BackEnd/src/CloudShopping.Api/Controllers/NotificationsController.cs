using CloudShopping.Api.Security;
using CloudShopping.Application.Features.Notifications.Commands.MarkNotificationRead;
using CloudShopping.Application.Features.Notifications.Commands.RetryNotification;
using CloudShopping.Application.Features.Notifications.Queries.GetCustomerNotifications;
using CloudShopping.Application.Features.Notifications.Queries.GetNotificationOutbox;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace CloudShopping.Api.Controllers;
[ApiController, Route("api/v1/store/notifications"), Authorize(Roles="Customer")]
public sealed class NotificationsController(ISender sender) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> List(int page=1,CancellationToken ct=default) => CommandResults.Respond(await sender.Send(new GetCustomerNotificationsQuery(StoreSecurity.Subject(User),page),ct), value => Ok(value));
    [HttpPost("{id}/read")]
    public async Task<IActionResult> Read(string id,CancellationToken ct) => CommandResults.Respond(await sender.Send(new MarkNotificationReadCommand(StoreSecurity.Subject(User),id),ct),_=>NoContent());
}
[ApiController, Route("api/v1/notifications"), Authorize(Roles="Administrator")]
public sealed class NotificationAdminController(ISender sender) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> List(int page=1,CancellationToken ct=default) => CommandResults.Respond(await sender.Send(new GetNotificationOutboxQuery(page),ct), value => Ok(value));
    [HttpPost("{id}/retry")]
    public async Task<IActionResult> Retry(string id,CancellationToken ct) => CommandResults.Respond(await sender.Send(new RetryNotificationCommand(id),ct),_=>NoContent());
}
