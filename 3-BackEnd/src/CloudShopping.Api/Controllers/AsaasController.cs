using System.ComponentModel.DataAnnotations;
using CloudShopping.Api.Security;
using CloudShopping.Infrastructure.Payments;
using CloudShopping.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace CloudShopping.Api.Controllers;

[ApiController,Route("api/v1/asaas")]
public sealed class AsaasController(AsaasAccounts accounts,AsaasPayments payments,AppDbContext db):ControllerBase
{
    [HttpGet("connection"),Authorize(Roles="Administrator")]
    public Task<object> Connection(CancellationToken ct)=>accounts.View(ct);
    [HttpPut("connection"),Authorize(Roles="Administrator")]
    public Task<object> Configure(ConnectionInput input,CancellationToken ct)=>accounts.Configure(input.Mode,input.Environment,input.ApiKey,input.WebhookToken,ct);
    [HttpGet("orders/{id:int}"),Authorize(Roles="Customer")]
    public Task<object> View(int id,CancellationToken ct)=>payments.View(id,StoreSecurity.Subject(User),ct);
    [HttpPost("orders/{id:int}"),Authorize(Roles="Customer")]
    public Task<object> Start(int id,PaymentInput input,CancellationToken ct)=>payments.Start(id,StoreSecurity.Subject(User),input.Method,ct);
    [HttpPost("orders/{id:int}/refresh"),Authorize(Roles="Customer")]
    public Task<object> Refresh(int id,CancellationToken ct)=>payments.Reconcile(id,StoreSecurity.Subject(User),ct);
    [HttpPost("orders/{id:int}/cancel"),Authorize(Roles="Customer")]
    public Task<object> Cancel(int id,CancellationToken ct)=>payments.Reconcile(id,StoreSecurity.Subject(User),ct,"cancel","Customer:"+StoreSecurity.Subject(User));
    [HttpGet("admin/payments"),Authorize(Roles="Administrator")]
    public async Task<object> List(int page=1,CancellationToken ct=default)
    {
        var rows=await db.Set<PaymentAttempt>().AsNoTracking().OrderByDescending(x=>x.CreatedAt).Skip((page-1)*20).Take(20).ToListAsync(ct);
        return rows.Select(AsaasPayments.Public);
    }
    [HttpPost("admin/orders/{id:int}/refresh"),Authorize(Roles="Administrator")]
    public Task<object> AdminRefresh(int id,CancellationToken ct)=>payments.Reconcile(id,null,ct);
    [HttpPost("admin/orders/{id:int}/cancel"),Authorize(Roles="Administrator")]
    public Task<object> AdminCancel(int id,CancellationToken ct)=>payments.Reconcile(id,null,ct,"cancel","Administrator:"+StoreSecurity.Subject(User));
    [HttpPost("admin/orders/{id:int}/refund"),Authorize(Roles="Administrator")]
    public Task<object> Refund(int id,CancellationToken ct)=>payments.Reconcile(id,null,ct,"refund","Administrator:"+StoreSecurity.Subject(User));
    [HttpPost("admin/orders/{id:int}/recover"),Authorize(Roles="Administrator")]
    public Task<object> Recover(int id,RecoverPaymentInput input,CancellationToken ct)=>payments.Recover(id,input.PaymentId,"Administrator:"+StoreSecurity.Subject(User),ct);
    [HttpGet("admin/orders/{id:int}/operations"),Authorize(Roles="Administrator")]
    public async Task<object> Operations(int id,CancellationToken ct)=>await db.Set<PaymentOperation>().Where(x=>x.OrderId==id).OrderBy(x=>x.RequestedAt).Select(x=>new{x.Kind,x.RequestedBy,x.RequestedAt}).ToListAsync(ct);
}
public sealed record ConnectionInput([Required,RegularExpression("^(Direct|PlatformSplit)$")]string Mode,
    [Required,RegularExpression("^(Sandbox|Production)$")]string Environment,[MaxLength(1000)]string? ApiKey,[MaxLength(255)]string? WebhookToken);
public sealed record PaymentInput([Required,RegularExpression("^(PIX|BOLETO|CREDIT_CARD)$")]string Method);
public sealed record RecoverPaymentInput([Required,MaxLength(100),RegularExpression("^pay_[A-Za-z0-9_-]+$")]string PaymentId);

[AttributeUsage(AttributeTargets.Class)]
public sealed class AsaasWebhookAttribute:Attribute;
[ApiController,AllowAnonymous,AsaasWebhook,Route("api/webhooks/asaas")]
public sealed class AsaasWebhookController(AsaasInbox inbox):ControllerBase
{
    [HttpPost("{endpoint}"),RequestSizeLimit(200_000)]
    public async Task<IActionResult> Receive(string endpoint,CancellationToken ct)
    {
        using var reader=new StreamReader(Request.Body);
        var payload=await reader.ReadToEndAsync(ct);
        await inbox.Receive(endpoint,Request.Headers["asaas-access-token"].ToString(),payload,ct);
        return Ok(new {received=true});
    }
}
