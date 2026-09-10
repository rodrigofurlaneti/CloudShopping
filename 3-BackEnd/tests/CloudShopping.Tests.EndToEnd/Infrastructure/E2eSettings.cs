using System.Text.Json;

namespace CloudShopping.Tests.E2E.Infrastructure;

internal static class E2eSettings
{
    // Environment variables override local settings, including on CI.
    internal static string? Get(string name)
    {
        if(Environment.GetEnvironmentVariable(name) is { Length: >0 } value)return value;
        var path=Path.Combine(AppContext.BaseDirectory,"e2e.local.json");
        if(!File.Exists(path))return null;
        using var document=JsonDocument.Parse(File.ReadAllText(path));
        return document.RootElement.TryGetProperty(name,out var setting) && setting.ValueKind==JsonValueKind.String
            ? setting.GetString():null;
    }
}
