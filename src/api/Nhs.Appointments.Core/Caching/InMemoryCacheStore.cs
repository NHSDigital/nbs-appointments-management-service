using Microsoft.Extensions.Caching.Memory;

namespace Nhs.Appointments.Core.Caching;

public class InMemoryCacheStore(IMemoryCache memoryCache) : ICacheStore
{
    public Task<bool> TryGetAsync<T>(string key, out T value) => Task.FromResult(memoryCache.TryGetValue(key, out value));
    public Task SetAsync<T>(string key, T value, DateTimeOffset absoluteExpiration) => Task.FromResult(memoryCache.Set(key, value, absoluteExpiration));
    public Task SetAsync<T>(string key, T value, TimeSpan expirationRelativeToNow) => Task.FromResult(memoryCache.Set(key, value, expirationRelativeToNow));
}
