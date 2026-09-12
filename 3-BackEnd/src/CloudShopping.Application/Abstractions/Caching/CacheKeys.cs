namespace CloudShopping.Application.Abstractions.Caching
{
    // Nomenclatura central das chaves de cache, sempre particionada por tenant
    // (tenant:{TenantId}:...) conforme exigido pela tarefa de cache Redis — evita que um
    // tenant veja dados cacheados de outro e permite invalidação/observação por tenant.
    public static class CacheKeys
    {
        public const string Departments = "departments";
        public const string Products = "products";
        public const string StoreBanners = "banners";
        public const string AccessProfiles = "access-profiles";

        // Entidade única de um tenant: tenant:{TenantId}:{entity}:{id}
        public static string TenantEntity(int tenantId, string entity, object id) => $"tenant:{tenantId}:{entity}:{id}";

        // Listagem completa de uma entidade para um tenant: tenant:{TenantId}:{entity}:list
        public static string TenantList(int tenantId, string entity) => $"tenant:{tenantId}:{entity}:list";

        // Prefixo usado para invalidação em massa via RemoveByPrefixAsync.
        public static string TenantPrefix(int tenantId, string entity) => $"tenant:{tenantId}:{entity}:";

        // Produtos também são consultados por SKU (ficha do produto/checkout) — chave
        // própria para não colidir com a busca por Id.
        public static string ProductBySku(int tenantId, string sku) => $"tenant:{tenantId}:products:sku:{sku.Trim().ToLowerInvariant()}";

        // Cache de Sessão (authsessions): TTL exato = ExpiresAt - agora, ver SessionStore.
        public static string SessionKey(int tenantId, string sessionId) => $"tenant:{tenantId}:session:{sessionId}";
    }

    // Faixas de TTL descritas na tarefa de cache: "Cache Longo" para catálogo/estrutura,
    // "Cache Médio" para ACL administrativa. O cache de sessão não usa TTL fixo — usa o
    // ExpiresAt exato de cada sessão (calculado em SessionStore, não aqui).
    public static class CacheTtl
    {
        public static readonly System.TimeSpan Long = System.TimeSpan.FromHours(6);
        public static readonly System.TimeSpan Medium = System.TimeSpan.FromMinutes(30);
        public static readonly System.TimeSpan Short = System.TimeSpan.FromMinutes(10);
    }
}
