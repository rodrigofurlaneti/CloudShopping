using System.Text.Json;
using CloudShopping.Application.Abstractions.Services;
namespace CloudShopping.Infrastructure.Services;
public sealed class ViaCepLookup(HttpClient client) : IPostalCodeLookup
{
    public async Task<PostalAddress?> FindAsync(string zipCode, CancellationToken ct)
    {
        if (!System.Text.RegularExpressions.Regex.IsMatch(zipCode, "^[0-9]{8}$"))
            throw new ArgumentException("Informe um CEP com 8 números.");
        using var response = await client.GetAsync($"ws/{zipCode}/json/", ct);
        response.EnsureSuccessStatusCode();
        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync(ct));
        var value = json.RootElement;
        if (value.TryGetProperty("erro", out var error) && error.ToString().Equals("true", StringComparison.OrdinalIgnoreCase)) return null;
        string Read(string key) => value.TryGetProperty(key, out var item) ? item.GetString() ?? "" : "";
        if (string.IsNullOrWhiteSpace(Read("localidade")) || Read("uf").Length != 2)
            throw new JsonException("Resposta de CEP inválida.");
        return new(zipCode, Read("logradouro"), Read("bairro"), Read("localidade"), Read("uf"));
    }
}
