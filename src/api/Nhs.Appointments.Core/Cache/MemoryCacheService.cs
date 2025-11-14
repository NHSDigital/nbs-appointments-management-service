using Microsoft.Extensions.Caching.Memory;

namespace Nhs.Appointments.Core.Cache;

public class MemoryCacheService<TModel>(IMemoryCache memoryCache) : ICacheService<TModel>
{
    private readonly SemaphoreSlim _cacheLock = new(1, 1);
    
    public bool TryGetCache<TCacheModel>(string key, out TCacheModel model)
    {
        return memoryCache.TryGetValue(key, out model);
    }

    public async Task SetCache<TCacheModel>(string key, TCacheModel model, DateTimeOffset expiration)
    {
        await _cacheLock.WaitAsync();
        
        try
        {
            memoryCache.Set(key, model, expiration);
        }
        finally
        {
            _cacheLock.Release();
        }
    }

    public async Task RemoveCache(string key)
    {
        await _cacheLock.WaitAsync();
        
        try
        {
            memoryCache.Remove(key);
        }
        finally
        {
            _cacheLock.Release();
        }
    }

    public async Task PatchCache(string key, TModel patch, Predicate<TModel> predicate, DateTimeOffset expiration)
    {
        await _cacheLock.WaitAsync();
        
        try
        {
            if (!TryGetCache<List<TModel>>(key, out var cache))
            {
                return;
            }
            var existingIndex = cache.FindIndex(predicate);
        
            if (existingIndex >= 0)
            {
                if (patch != null)
                {
                    cache[existingIndex] = patch;
                }
                else
                {
                    cache.RemoveAt(existingIndex);
                }
            }
            else if (patch != null)
            {
                cache.Add(patch);
            }
            
            memoryCache.Set(key, cache, expiration);
        }
        finally
        {
            _cacheLock.Release();
        }

    }
}
