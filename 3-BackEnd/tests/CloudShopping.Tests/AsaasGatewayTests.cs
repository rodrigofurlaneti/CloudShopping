using System.Net;
using System.Text;
using CloudShopping.Infrastructure.Payments;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Configuration;
using Xunit;

public sealed class AsaasGatewayTests
{
    [Fact]
    public async Task Gateway_uses_fixed_sandbox_host_auth_header_and_never_exposes_provider_body_in_errors()
    {
        var protection=new EphemeralDataProtectionProvider();var handler=new CaptureHandler();using var http=new HttpClient(handler);
        var gateway=new AsaasGateway(http,protection,new ConfigurationBuilder().Build());
        var account=new AsaasConnection{ProtectedApiKey=protection.CreateProtector("Asaas.Credentials.v1").Protect("synthetic-private-key")};
        var ex=await Assert.ThrowsAsync<AsaasApiException>(()=>gateway.Send(account,HttpMethod.Post,"payments",new{value=115},default));
        Assert.Equal("https://api-sandbox.asaas.com/v3/payments",handler.Url);Assert.Equal("synthetic-private-key",handler.Token);
        Assert.DoesNotContain("sensitive-provider-body",ex.Message);Assert.DoesNotContain("synthetic-private-key",ex.Message);
    }
    [Fact]
    public async Task Production_and_arbitrary_hosts_are_rejected_before_http()
    {
        var protection=new EphemeralDataProtectionProvider();var handler=new CaptureHandler();using var http=new HttpClient(handler);
        var gateway=new AsaasGateway(http,protection,new ConfigurationBuilder().Build());
        await Assert.ThrowsAsync<InvalidOperationException>(()=>gateway.Send(new(){Environment="Production"},HttpMethod.Get,"payments",null,default));
        await Assert.ThrowsAsync<ArgumentException>(()=>gateway.Send(new(),HttpMethod.Get,"https://other.example/payments",null,default));
        Assert.Null(handler.Url);
    }
    private sealed class CaptureHandler:HttpMessageHandler
    {
        public string? Url,Token;
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,CancellationToken ct)
        {Url=request.RequestUri!.AbsoluteUri;Token=request.Headers.GetValues("access_token").Single();return Task.FromResult(new HttpResponseMessage(HttpStatusCode.BadRequest){Content=new StringContent("sensitive-provider-body",Encoding.UTF8)});}
    }
}
