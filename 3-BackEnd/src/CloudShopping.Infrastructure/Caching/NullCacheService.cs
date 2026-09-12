using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CloudShopping.Application.Abstractions.Caching;

namespace CloudShopping.Infrastructure.Caching
{
    // Fallback "no-op": usado quando o Redis não está configurado (sem
    // ConnectionStrings:Redis) ou quando a conexão inicial falhou. A aplicação continua
    // funcionando normalmente, apenas sem ganho de cache — nunca deve lançar exceção.
    public sealed class NullCacheService : ICacheService
    {
        public Task<T?> GetAsync<T>(string key, CancellationToken ct = default) where T : class => Task.FromResult<T?>(null);

        public Task SetAsync<T>(string key, T value, TimeSpan ttl, CancellationToken ct = default) where T : class => Task.CompletedTask;

        public Task<T?> GetOrCreateAsync<T>(string key, TimeSpan ttl, Func<CancellationToken, Task<T?>> factory, CancellationToken ct = default) where T : class
            => factory(ct);

        public Task RemoveAsync(string key, CancellationToken ct = default) => Task.CompletedTask;

        public Task RemoveManyAsync(IEnumerable<string> keys, CancellationToken ct = default) => Task.CompletedTask;

        public Task RemoveByPrefixAsync(string prefix, CancellationToken ct = default) => Task.CompletedTask;
    }
}
