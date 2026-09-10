using CloudShopping.Api.Security;
using Microsoft.AspNetCore.Http;
using Xunit;

public sealed class RequestCancellationTests
{
    [Fact]
    public async Task Already_cancelled_request_does_not_start_authentication()
    {
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        var context = new DefaultHttpContext { RequestAborted = cancellation.Token };
        var called = false;
        var middleware = new RequestCancellationMiddleware(_ => { called = true; return Task.CompletedTask; });

        await middleware.InvokeAsync(context);

        Assert.False(called);
        Assert.Equal(499, context.Response.StatusCode);
        Assert.Empty(context.Response.Headers);
    }

    [Fact]
    public async Task Cancellation_during_authentication_ends_request_without_clearing_cookie()
    {
        using var cancellation = new CancellationTokenSource();
        var context = new DefaultHttpContext { RequestAborted = cancellation.Token };
        var middleware = new RequestCancellationMiddleware(_ =>
        {
            cancellation.Cancel();
            return Task.FromCanceled(cancellation.Token);
        });

        await middleware.InvokeAsync(context);

        Assert.Equal(499, context.Response.StatusCode);
        Assert.False(context.Response.Headers.ContainsKey("Set-Cookie"));
    }

    [Fact]
    public async Task Unrelated_cancellation_is_not_hidden()
    {
        var failure = new OperationCanceledException("Internal operation cancelled");
        var middleware = new RequestCancellationMiddleware(_ => Task.FromException(failure));
        var actual = await Assert.ThrowsAsync<OperationCanceledException>(() => middleware.InvokeAsync(new DefaultHttpContext()));
        Assert.Same(failure, actual);
    }

    [Fact]
    public async Task Successful_request_keeps_its_response()
    {
        var context = new DefaultHttpContext();
        var middleware = new RequestCancellationMiddleware(ctx => { ctx.Response.StatusCode = 204; return Task.CompletedTask; });
        await middleware.InvokeAsync(context);
        Assert.Equal(204, context.Response.StatusCode);
    }
}
