using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace CloudShopping.Application.Abstractions.Caching
{
    // Abstração de cache usada pelas Query/Command Handlers (nunca pelos Controllers
    // diretamente, mantendo o cache dentro da camada de Application, junto da regra de
    // negócio que decide o que é cacheável). Implementada por RedisCacheService quando o
    // Redis está disponível e por NullCacheService (no-op) quando não está configurado ou
    // a conexão falha — a aplicação nunca deve deixar de funcionar por causa do cache.
    public interface ICacheService
    {
        Task<T?> GetAsync<T>(string key, CancellationToken ct = default) where T : class;

        Task SetAsync<T>(string key, T value, TimeSpan ttl, CancellationToken ct = default) where T : class;

        // Busca no cache; em caso de ausência, executa "factory", grava o resultado (se
        // não nulo) com o TTL informado e o devolve. Uso típico dentro de Query Handlers:
        // uma única linha substitui o par manual Get/Set.
        Task<T?> GetOrCreateAsync<T>(string key, TimeSpan ttl, Func<CancellationToken, Task<T?>> factory, CancellationToken ct = default) where T : class;

        Task RemoveAsync(string key, CancellationToken ct = default);

        Task RemoveManyAsync(IEnumerable<string> keys, CancellationToken ct = default);

        // Remove todas as chaves que começam com "prefix" (após aplicar o prefixo de
        // instância). Usado para invalidações amplas (ex.: todas as páginas cacheadas de
        // uma listagem). Custa uma varredura (SCAN) no Redis — usar com moderação, só em
        // Command Handlers (nunca em caminhos de leitura).
        Task RemoveByPrefixAsync(string prefix, CancellationToken ct = default);
    }
}
