namespace FinanceTracker.Application.Interfaces;

public interface IRedisService
{
    Task<T?> GetAsync<T>(string key);
    Task SetAsync(string key, object data, TimeSpan? expiry = null);
    Task RemoveAsync(string key);
    Task RemoveByPatternAsync(string pattern);
}
