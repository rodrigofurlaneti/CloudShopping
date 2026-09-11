using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Configuration;
using MySqlConnector;
using System.Security.Cryptography;
using System.Text;
namespace CloudShopping.Infrastructure.Payments;

public interface IAsaasGateway
{
    Task<JsonElement> Send(AsaasConnection account, HttpMethod method, string path, object? body, CancellationToken ct);
}
public sealed class AsaasApiException(int status) : Exception("O Asaas não concluiu a operação (HTTP " + status + ").")
{ public int Status {get;} = status; }
public sealed class AsaasGateway(HttpClient http, IDataProtectionProvider protection, IConfiguration config) : IAsaasGateway
{
    public async Task<JsonElement> Send(AsaasConnection account, HttpMethod method, string path, object? body, CancellationToken ct)
    {
        if (account.Environment is not ("Sandbox" or "Production")) throw new InvalidOperationException("Ambiente inválido.");
        if (account.Environment=="Production" && !config.GetValue<bool>("Asaas:AllowProduction")) throw new InvalidOperationException("Produção Asaas não habilitada no servidor.");
        var host=account.Environment=="Sandbox"?"https://api-sandbox.asaas.com/v3/":"https://api.asaas.com/v3/";
        if(path.StartsWith('/') || path.Contains("://")) throw new ArgumentException("Rota inválida.");
        using var req=new HttpRequestMessage(method,host+path);
        req.Headers.Add("access_token",protection.CreateProtector("Asaas.Credentials.v1").Unprotect(account.ProtectedApiKey));
        req.Headers.UserAgent.ParseAdd("CloudShopping/2.0");
        if(body!=null) req.Content=JsonContent.Create(body);
        using var timeout=CancellationTokenSource.CreateLinkedTokenSource(ct);timeout.CancelAfter(TimeSpan.FromSeconds(20));
        using var response=await http.SendAsync(req,timeout.Token);
        if(!response.IsSuccessStatusCode) throw new AsaasApiException((int)response.StatusCode);
        var text=await response.Content.ReadAsStringAsync(timeout.Token);
        using var doc=JsonDocument.Parse(string.IsNullOrWhiteSpace(text)?"{}":text);
        return doc.RootElement.Clone();
    }
}
public static class JsonFields
{
    public static string? Text(this JsonElement j,string key)=>j.ValueKind==JsonValueKind.Object && j.TryGetProperty(key,out var v) && v.ValueKind==JsonValueKind.String ? v.GetString():null;
    public static decimal Money(this JsonElement j,string key)=>j.ValueKind==JsonValueKind.Object&&j.TryGetProperty(key,out var v)&&v.ValueKind==JsonValueKind.Number&&v.TryGetDecimal(out var d)?d:0;
    public static JsonElement[] Rows(this JsonElement j)=>j.ValueKind==JsonValueKind.Array?j.EnumerateArray().ToArray():j.ValueKind==JsonValueKind.Object&&j.TryGetProperty("data",out var v)&&v.ValueKind==JsonValueKind.Array?v.EnumerateArray().ToArray():throw new InvalidOperationException("Lista Asaas sem dados; operação requer conciliação.");
    public static string Id(this JsonElement j)=>j.Text("id")??throw new InvalidOperationException("Resposta Asaas sem identificador; operação requer conciliação.");
    public static string Hash(string value)=>Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value)));
    public static bool Verify(string value,string hash)=>hash.Length==64 && CryptographicOperations.FixedTimeEquals(Encoding.ASCII.GetBytes(Hash(value)),Encoding.ASCII.GetBytes(hash));
    public static string Reference(PaymentAttempt attempt)=>"csp_"+attempt.Id;
    public static string? SafeUrl(string? url,string environment)
    {
        if(!Uri.TryCreate(url,UriKind.Absolute,out var uri)||uri.Scheme!="https")return null;
        var allowed=environment=="Sandbox"?new[]{"sandbox.asaas.com","www.sandbox.asaas.com"}:new[]{"asaas.com","www.asaas.com"};
        return allowed.Contains(uri.Host)?uri.AbsoluteUri:null;
    }
    public static string CheckoutUrl(string environment,string id)=>
        (environment=="Sandbox"?"https://sandbox.asaas.com":"https://asaas.com")+"/checkoutSession/show?id="+Uri.EscapeDataString(id);
}
// A database-wide lock coordinates API instances and workers; no transaction is held during HTTP calls.
public sealed class PaymentLock : IAsyncDisposable
{
    private readonly MySqlConnection connection;
    private readonly string name;
    private PaymentLock(MySqlConnection c,string n){connection=c;name=n;}
    public static async Task<PaymentLock> Acquire(string connectionString,string resource,CancellationToken ct)
    {
        var c=new MySqlConnection(connectionString);await c.OpenAsync(ct);
        var name=JsonFields.Hash(c.Database+":"+resource);
        try {
            using var cmd=new MySqlCommand("SELECT GET_LOCK(@name,5)",c);cmd.Parameters.AddWithValue("@name",name);
            if(Convert.ToInt32(await cmd.ExecuteScalarAsync(ct))!=1) throw new InvalidOperationException("Operação em andamento. Atualize em alguns segundos.");
            return new(c,name);
        } catch {await c.DisposeAsync();throw;}
    }
    public async ValueTask DisposeAsync()
    {
        try {using var cmd=new MySqlCommand("SELECT RELEASE_LOCK(@name)",connection);cmd.Parameters.AddWithValue("@name",name);await cmd.ExecuteScalarAsync();}
        finally {await connection.DisposeAsync();}
    }
}
