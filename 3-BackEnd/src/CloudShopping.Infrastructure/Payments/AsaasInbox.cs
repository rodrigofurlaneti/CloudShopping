using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Configuration;
using MySqlConnector;
using System.Text.Json;
namespace CloudShopping.Infrastructure.Payments;

public sealed class AsaasInbox(IConfiguration config,IDataProtectionProvider protection)
{
    public string ConnectionString=>config.GetConnectionString("DefaultConnection")??throw new InvalidOperationException("Banco não configurado.");
    public string Decode(string value)=>protection.CreateProtector("Asaas.Inbox.v1").Unprotect(value);
    public async Task Receive(string endpoint,string token,string payload,CancellationToken ct)
    {
        if(token.Length is <32 or >255)throw new UnauthorizedAccessException();
        if(payload.Length>200_000)throw new ArgumentException("Evento excede limite.");
        using var doc=JsonDocument.Parse(payload);var root=doc.RootElement;
        var id=root.Text("id");var type=root.Text("event");
        if(string.IsNullOrWhiteSpace(id)||id.Length>150||string.IsNullOrWhiteSpace(type)||type.Length>80)throw new ArgumentException("Identificação do evento inválida.");
        await using var c=new MySqlConnection(ConnectionString);await c.OpenAsync(ct);
        using var lookup=new MySqlCommand(endpoint.StartsWith("platform-")?
            "SELECT DISTINCT AccountKey,WebhookTokenHash FROM asaasconnections WHERE Mode='PlatformSplit' AND Environment=@env AND WebhookTokenHash=@hash":
            "SELECT AccountKey,WebhookTokenHash FROM asaasconnections WHERE Id=@id",c);
        lookup.Parameters.AddWithValue("@env",endpoint=="platform-sandbox"?"Sandbox":endpoint=="platform-production"?"Production":"invalid");
        lookup.Parameters.AddWithValue("@hash",JsonFields.Hash(token));lookup.Parameters.AddWithValue("@id",endpoint);
        string account;
        await using(var r=await lookup.ExecuteReaderAsync(ct))
        {
            if(!await r.ReadAsync(ct)||!JsonFields.Verify(token,r.GetString(1)))throw new UnauthorizedAccessException();
            account=r.GetString(0);if(await r.ReadAsync(ct))throw new UnauthorizedAccessException();
        }
        using var insert=new MySqlCommand("INSERT INTO asaasinbox(AccountKey,EventId,EventType,ProtectedPayload,ReceivedAt,NextCheckAt) VALUES(@account,@event,@type,@payload,UTC_TIMESTAMP(6),UTC_TIMESTAMP(6)) ON DUPLICATE KEY UPDATE EventId=EventId",c);
        insert.Parameters.AddWithValue("@account",account);insert.Parameters.AddWithValue("@event",id);insert.Parameters.AddWithValue("@type",type);
        insert.Parameters.AddWithValue("@payload",protection.CreateProtector("Asaas.Inbox.v1").Protect(payload));
        await insert.ExecuteNonQueryAsync(ct);
    }
}
