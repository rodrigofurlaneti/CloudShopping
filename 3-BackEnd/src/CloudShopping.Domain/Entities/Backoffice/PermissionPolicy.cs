namespace CloudShopping.Domain.Entities.Backoffice;

public static class PermissionPolicy
{
    public const string GeneralAdministrator = "Administrador Geral";
    public static IReadOnlyList<string> Available { get; } = Array.AsReadOnly(
        new[] { "catalog", "orders", "customers", "finance", "settings", "support", "moderation", "promotions", "reports" }
            .SelectMany(x => new[] { x + ".read", x + ".write" }).Append("stock.write").Append("finance.refund").ToArray());

    public static bool Allows(IEnumerable<string> permissions, string? required) => permissions.Contains("*") || required != null && permissions.Contains(required);

    public static string[] Normalize(string[] desired)
    {
        if (desired == null || desired.Length > Available.Count || desired.Any(x => !Available.Contains(x)))
            throw new ArgumentException("Permissões inválidas.");
        var normalized = desired.Distinct().Order().ToArray();
        if (normalized.Any(x => x.EndsWith(".write") && x != "stock.write" && !normalized.Contains(x.Replace(".write", ".read"))))
            throw new ArgumentException("Inclua a leitura do módulo antes de habilitar alterações.");
        if (normalized.Contains("stock.write") && !normalized.Contains("catalog.read") || normalized.Contains("finance.refund") && !normalized.Contains("finance.read"))
            throw new ArgumentException("Inclua a leitura de catálogo/financeiro para estoque/estorno.");
        return normalized;
    }
}
