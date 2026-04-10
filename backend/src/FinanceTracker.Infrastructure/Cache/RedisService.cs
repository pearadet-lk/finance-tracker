using FinanceTracker.Application.Interfaces;
using StackExchange.Redis;
using System.Text.Json;

namespace FinanceTracker.Infrastructure.Cache;

public class RedisService : IRedisService
{
    private readonly IDatabase _db;
    private readonly IServer _server;

    public RedisService(IConnectionMultiplexer redis)
    {
        _db = redis.GetDatabase();
        _server = redis.GetServer(redis.GetEndPoints().First());
    }

    public async Task<T?> GetAsync<T>(string key)
    {
        var value = await _db.StringGetAsync(key);
        if (!value.HasValue) return default;
        try
        {
            return JsonSerializer.Deserialize<T>(value.ToString());
        }
        catch (JsonException)
        {
            // Cached payload shape may change across deployments; evict bad entry and refresh from source.
            await _db.KeyDeleteAsync(key);
            return default;
        }
        catch (InvalidOperationException)
        {
            await _db.KeyDeleteAsync(key);
            return default;
        }
    }

    public async Task SetAsync(string key, object data, TimeSpan? expiry = null)
    {
        var json = JsonSerializer.Serialize(data);
        await _db.StringSetAsync(key, json, expiry ?? TimeSpan.FromMinutes(10));
    }

    public async Task RemoveAsync(string key)
    {
        await _db.KeyDeleteAsync(key);
    }

    public async Task RemoveByPatternAsync(string pattern)
    {
        var keys = _server.Keys(pattern: $"{pattern}*").ToArray();
        if (keys.Any())
            await _db.KeyDeleteAsync(keys);
    }
}
