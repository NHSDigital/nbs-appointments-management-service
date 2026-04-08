using System.Collections.Concurrent;

namespace Nhs.Appointments.Core.Caching;

public class CacheService(ICacheStore cacheStore, TimeProvider timeProvider) : ICacheService
{
    private static readonly ConcurrentDictionary<string, SemaphoreSlim> LazySlidingCacheLocks = new();
    
    public async Task<T> GetLazySlidingCacheValue<T>(string cacheKey, LazySlideCacheOptions<T> options)
    {
        //intentionally prefix cache key indicating that it is a lazy sliding value
        var lazySlideCacheKey = CacheKeys.LazySlideCacheKey(cacheKey);
        
        var lazySlideCacheLock =
            LazySlidingCacheLocks.GetOrAdd(lazySlideCacheKey, _ => new SemaphoreSlim(1, 1));
        
        //we want to wait until the lock is free before continuing
        await lazySlideCacheLock.WaitAsync();

        var slidePerformed = false;

        try
        {
            if (await cacheStore.TryGetAsync<LazySlideCacheObject<T>>(lazySlideCacheKey, out var lazySlideCacheObject))
            {
                ArgumentNullException.ThrowIfNull(lazySlideCacheObject);
            
                if (lazySlideCacheObject.IsFresh(options.SlideThreshold, timeProvider.GetUtcNow()))
                {
                    return lazySlideCacheObject.Value;
                }
                    
                /*
                    Update the cache value so the NEXT request gets a newer version of the latest expensive value fetch
                    This approach means the cache entry is never guaranteed to be the exact latest value (unless a cache value does not exist) - but it is recent enough to not have a big impact
                    The performance gain is a sufficient benefit to the value being potentially slightly behind the latest value 
                */
                _ = SlideCache(lazySlideCacheKey, options, lazySlideCacheLock, lazySlideCacheObject.Value);
                slidePerformed = true;

                return lazySlideCacheObject.Value;
            }
            
            return await SetCacheAndReturn(lazySlideCacheKey, options, (value) => new LazySlideCacheObject<T>(value, timeProvider.GetUtcNow()));
        }
        finally
        {
            //the lock was released earlier if a slide was performed
            if (!slidePerformed)
            {
                lazySlideCacheLock.Release();
            }
        }
    }

    public async Task<T> GetCacheValue<T>(string cacheKey, CacheOptions<T> options)
    {
        if (await cacheStore.TryGetAsync<CacheObject<T>>(cacheKey, out var cacheObj))
        {
            ArgumentNullException.ThrowIfNull(cacheObj);
            if (cacheObj.Value != null)
            {
                return cacheObj.Value;
            }
        }
        
        return await SetCacheAndReturn(cacheKey, options, (value) => new CacheObject<T>(value));
    }

    public async Task<T> GetCacheValueWithDefault<T>(string cacheKey, CacheOptions<T> options, T defaultValue)
    {
        if (await cacheStore.TryGetAsync<CacheObject<T>>(cacheKey, out var cacheObj))
        {
            ArgumentNullException.ThrowIfNull(cacheObj);
            if (cacheObj.Value != null)
            {
                return cacheObj.Value;
            }
        }
        
        var tryResult = await TryPattern.TryAsync(options.UpdateOperation);
            
        if (!tryResult.Completed)
        {
            return defaultValue;
        }

        await cacheStore.SetAsync(cacheKey, new CacheObject<T>(tryResult.Result), options.AbsoluteExpiration);
        return tryResult.Result;
    }

    private async Task<T> SetCacheAndReturn<T, TCache>(string cacheKey, CacheOptions<T> options, Func<T, TCache> initialiseCacheObject)
    {
        var newValue = await options.UpdateOperation();
        await cacheStore.SetAsync(cacheKey, initialiseCacheObject(newValue), options.AbsoluteExpiration);
        return newValue;
    }

    private async Task SlideCache<T>(string lazySlideCacheKey, LazySlideCacheOptions<T> options, SemaphoreSlim lazySlideCacheLock, T lazyValue)
    {
        try
        {
            //update the cache datetime prematurely so that concurrent waiting threads do not trigger their own slide operation
            await cacheStore.SetAsync(lazySlideCacheKey, new LazySlideCacheObject<T>(lazyValue, timeProvider.GetUtcNow()),
                options.AbsoluteExpiration);
        }
        finally
        {
            //can release other threads now that the cache time has been updated
            lazySlideCacheLock.Release();
        }
        
        //then update the actual value now that no locks are being held
        await SetCacheAndReturn(lazySlideCacheKey, options, (value) => new LazySlideCacheObject<T>(value, timeProvider.GetUtcNow()));
    }
}
