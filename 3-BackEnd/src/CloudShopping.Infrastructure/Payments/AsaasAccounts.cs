using CloudShopping.Infrastructure.Persistence;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Text.Json;
namespace CloudShopping.Infrastructure.Payments;

public sealed class AsaasAccounts(AppDbContext db,IAsaasGateway gateway,IDataProtectionProvider protection,IConfiguration config)
{
    public async Task<object> View(CancellationToken ct)
    {
        var current=await db.Set<AsaasConnection>().SingleOrDefaultAsync(x=>x.Enabled,ct);
        return new {configured=current!=null, mode=current?.Mode, environment=current?.Environment,
            walletId=current?.WalletId, webhookPath=current==null?null:WebhookPath(current),
            productionAllowed=config.GetValue<bool>("Asaas:AllowProduction"),
            merchantWalletId=config[$"Asaas:Platform:{current?.Environment??"Sandbox"}:MerchantWallets:{db.CurrentTenantId}"],
            merchantPercent=config[$"Asaas:Platform:{current?.Environment??"Sandbox"}:MerchantPercent:{db.CurrentTenantId}"]};
    }
    public static string WebhookPath(AsaasConnection a)=>"/api/webhooks/asaas/"+(a.Mode=="PlatformSplit"?"platform-"+a.Environment.ToLowerInvariant():a.Id);
    public async Task<object> Configure(string mode,string environment,string? apiKey,string? webhookToken,CancellationToken ct)
    {
        if(mode is not("Direct" or "PlatformSplit")||environment is not("Sandbox" or "Production"))throw new ArgumentException("Modo/ambiente inválido.");
        if(environment=="Production"&&!config.GetValue<bool>("Asaas:AllowProduction"))throw new InvalidOperationException("Produção não habilitada no servidor.");
        if(mode=="PlatformSplit") {apiKey=config[$"Asaas:Platform:{environment}:ApiKey"];webhookToken=config[$"Asaas:Platform:{environment}:WebhookToken"];}
        if(string.IsNullOrWhiteSpace(apiKey)||apiKey.Length>1000||apiKey.Any(char.IsWhiteSpace))throw new ArgumentException("Informe uma API Key válida para este ambiente.");
        if(webhookToken==null||webhookToken.Length is <32 or >255||webhookToken.Any(char.IsWhiteSpace)||webhookToken==apiKey)throw new ArgumentException("Configure um token de webhook independente, entre 32 e 255 caracteres, sem espaços.");
        var a=new AsaasConnection {TenantId=db.CurrentTenantId,Mode=mode,Environment=environment,
            ProtectedApiKey=protection.CreateProtector("Asaas.Credentials.v1").Protect(apiKey),WebhookTokenHash=JsonFields.Hash(webhookToken)};
        var wallets=await gateway.Send(a,HttpMethod.Get,"wallets/",null,ct);
        a.WalletId=wallets.Rows().Single().Id();a.AccountKey=environment+":"+a.WalletId;
        if(mode=="PlatformSplit"&&await db.Set<AsaasConnection>().IgnoreQueryFilters().AnyAsync(x=>x.Mode==mode&&x.Environment==environment&&x.AccountKey!=a.AccountKey,ct))
            throw new InvalidOperationException("A conta emissora da plataforma não pode ser trocada por esta tela. É necessária migração financeira.");
        if(mode=="PlatformSplit") _=Split(a);
        await using var lease=await PaymentLock.Acquire(db.Database.GetConnectionString()!,"account:"+db.CurrentTenantId,ct);
        var previous=await db.Set<AsaasConnection>().Where(x=>x.Enabled).ToListAsync(ct);
        foreach(var p in previous)p.Enabled=false;
        // A key rotation must keep old charges recoverable on the same verified issuer account.
        var sameAccount=await db.Set<AsaasConnection>().Where(x=>x.AccountKey==a.AccountKey).ToListAsync(ct);
        foreach(var historical in sameAccount)historical.ProtectedApiKey=a.ProtectedApiKey;
        db.Add(a);await db.SaveChangesAsync(ct);
        return await View(ct);
    }
    public string Split(AsaasConnection account)
    {
        if(account.Mode=="Direct")return "[]";
        var wallet=config[$"Asaas:Platform:{account.Environment}:MerchantWallets:{account.TenantId}"];
        var percent=config.GetValue<decimal?>($"Asaas:Platform:{account.Environment}:MerchantPercent:{account.TenantId}");
        if(!Guid.TryParse(wallet,out _)||wallet==account.WalletId||percent is null or <=0 or >100||decimal.Round(percent.Value,4)!=percent)
            throw new InvalidOperationException("A plataforma precisa configurar carteira do lojista e percentual do valor líquido (maior que 0 até 100).");
        return JsonSerializer.Serialize(new[]{new{walletId=wallet,percentualValue=percent.Value}});
    }
}
