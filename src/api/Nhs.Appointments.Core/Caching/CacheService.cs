using System.Collections.Concurrent;

namespace Nhs.Appointments.Core.Caching;

public class CacheService(ICacheStore cacheStore, TimeProvider timeProvider) : ICacheService
{
    private static readonly ConcurrentDictionary<string, SemaphoreSlim> LazySlidingCacheLocks = new();
    
    public async Task<T> GetLazySlidingCacheValue<T>(string cacheKey, LazySlideCacheOptions<T> options)
    {
        //intentionally prefix cache key indicating that it is a lazy sliding value
        var lazySlideCacheKey = CacheKey.LazySlideCacheKey(cacheKey);
        
        if (options.AbsoluteExpiration <= options.SlideThreshold)
        {
            throw new ArgumentException("Configuration is not supported, AbsoluteExpiration must be greater than the SlideThreshold");
        }
        
        var utcNow = timeProvider.GetUtcNow();
        
        var lazySlideCacheLock =
            LazySlidingCacheLocks.GetOrAdd(lazySlideCacheKey, _ => new SemaphoreSlim(1, 1));
        
        //we want to wait until the lock is free before continuing
        await lazySlideCacheLock.WaitAsync();

        var slidePerformed = false;

        try
        {
            if (await cacheStore.TryGetAsync<LazySlideCacheObject>(lazySlideCacheKey, out var lazySlideCacheObject))
            {
                ArgumentNullException.ThrowIfNull(lazySlideCacheObject);
            
                //check if we want to update the existing cache in the background lazily...
                if (lazySlideCacheObject.DateTimeUpdated.Add(options.SlideThreshold) < utcNow)
                {
                    //Sliding cache functionality
                    
                    //Update the cache value so the NEXT request gets a newer version of the latest expensive value fetch
                    //This approach means the cache entry is never guaranteed to be the exact latest value (unless a cache value does not exist) - but it is recent enough to not have a big impact
                    //The performance gain is a sufficient benefit to the value being potentially slightly behind the latest value
                    _ = SlideCache(lazySlideCacheKey, options, lazySlideCacheLock, (T)lazySlideCacheObject.Value, utcNow);
                    slidePerformed = true;
                }
            
                //return the current cached value regardless of whether sliding was invoked
                return (T)lazySlideCacheObject.Value;
            }
            
            var value = await options.UpdateOperation();
            await cacheStore.SetAsync(lazySlideCacheKey, new LazySlideCacheObject(value, utcNow),
                utcNow.Add(options.AbsoluteExpiration));
            return value;
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
        
        var newValue = await options.UpdateOperation();

        await cacheStore.SetAsync(cacheKey, new CacheObject<T>(newValue), options.AbsoluteExpiration);
        return newValue;
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

    private async Task SlideCache<T>(string lazySlideCacheKey, LazySlideCacheOptions<T> options, SemaphoreSlim lazySlideCacheLock, T lazyValue, DateTimeOffset dateTime)
    {
        try
        {
            //update the cache datetime prematurely so that concurrent waiting threads do not trigger their own slide operation
            await cacheStore.SetAsync(lazySlideCacheKey, new LazySlideCacheObject(lazyValue, dateTime),
                dateTime.Add(options.AbsoluteExpiration));
        }
        finally
        {
            //can release other threads now that the cache time has been updated
            lazySlideCacheLock.Release();
        }
        
        //then update the actual value now that no locks are being held
        var value = await options.UpdateOperation();
        await cacheStore.SetAsync(lazySlideCacheKey, new LazySlideCacheObject(value, dateTime),
            dateTime.Add(options.AbsoluteExpiration));
    }
}
