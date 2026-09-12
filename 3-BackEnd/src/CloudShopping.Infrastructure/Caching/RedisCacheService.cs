using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using CloudShopping.Application.Abstractions.Caching;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace CloudShopping.Infrastructure.Caching
{
    // Implementação do cache sobre o Redis (StackExchange.Redis). Toda operação é
    // protegida por try/catch: uma falha de rede/timeout do Redis nunca deve derrubar a
    // requisição do usuário — o handler chamador simplesmente segue como se fosse um
    // cache miss (ou, no caso de escrita/invalidação, como um no-op registrado em log).
    public sealed class RedisCacheService : ICacheService
    {
        private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

        private readonly IConnectionMultiplexer _multiplexer;
        private readonly string _prefix;
        private readonly ILogger<RedisCacheService> _logger;

        public RedisCacheService(IConnectionMultiplexer multiplexer, string instanceName, ILogger<RedisCacheService> logger)
        {
            _multiplexer = multiplexer;
            _prefix = string.IsNullOrWhiteSpace(instanceName) ? "cloudshopping:" : instanceName;
            _logger = logger;
        }

        private IDatabase Db => _multiplexer.GetDatabase();
        private string Key(string key) => _prefix + key;

        public async Task<T?> GetAsync<T>(string key, CancellationToken ct = default) where T : class
        {
            try
            {
                var value = await Db.StringGetAsync(Key(key));
                if (value.IsNullOrEmpty) return null;
                var json = (string?)value;
                return json is null ? null : JsonSerializer.Deserialize<T>(json, JsonOptions);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Falha ao ler o cache Redis (chave {Key}). Seguindo sem cache.", key);
                return null;
            }
        }

        public async Task SetAsync<T>(string key, T value, TimeSpan ttl, CancellationToken ct = default) where T : class
        {
            try
            {
                var json = JsonSerializer.Serialize(value, JsonOptions);
                await Db.StringSetAsync(Key(key), json, ttl);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Falha ao gravar o cache Redis (chave {Key}). Requisição segue sem cache.", key);
            }
        }

        public async Task<T?> GetOrCreateAsync<T>(string key, TimeSpan ttl, Func<CancellationToken, Task<T?>> factory, CancellationToken ct = default) where T : class
        {
            var cached = await GetAsync<T>(key, ct);
            if (cached is not null) return cached;

            var value = await factory(ct);
            if (value is not null) await SetAsync(key, value, ttl, ct);
            return value;
        }

        public async Task RemoveAsync(string key, CancellationToken ct = default)
        {
            try
            {
                await Db.KeyDeleteAsync(Key(key));
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Falha ao invalidar o cache Redis (chave {Key}).", key);
            }
        }

        public async Task RemoveManyAsync(IEnumerable<string> keys, CancellationToken ct = default)
        {
            try
            {
                var redisKeys = keys.Select(k => (RedisKey)Key(k)).ToArray();
                if (redisKeys.Length > 0) await Db.KeyDeleteAsync(redisKeys);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Falha ao invalidar múltiplas chaves do cache Redis.");
            }
        }

        public Task RemoveByPrefixAsync(string prefix, CancellationToken ct = default)
        {
            try
            {
                var fullPrefix = Key(prefix);
                var db = Db;
                foreach (var endpoint in _multiplexer.GetEndPoints())
                {
                    var server = _multiplexer.GetServer(endpoint);
                    if (server.IsReplica) continue;

                    // IServer.Keys usa SCAN internamente (não bloqueia o servidor Redis
                    // como o antigo KEYS faria); a enumeração aqui é síncrona no cliente,
                    // aceitável porque isso só roda em Command Handlers de invalidação.
                    var keys = server.Keys(database: db.Database, pattern: fullPrefix + "*").ToArray();
                    if (keys.Length > 0) db.KeyDelete(keys);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Falha ao invalidar o cache Redis pelo prefixo {Prefix}.", prefix);
            }
            return Task.CompletedTask;
        }
    }
}
