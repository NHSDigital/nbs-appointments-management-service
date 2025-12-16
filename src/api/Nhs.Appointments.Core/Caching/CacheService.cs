using System.Collections.Concurrent;
using Microsoft.Extensions.Caching.Memory;

namespace Nhs.Appointments.Core.Caching;

public record LazySlideCacheOptions<T>(string CacheKey, Func<Task<T>> UpdateOperation, TimeSpan SlideThreshold, TimeSpan AbsoluteExpiration);

public interface ICacheService
{
    Task<T> GetLazySlidingCacheValue<T>(LazySlideCacheOptions<T> options);
}

public class CacheService(IMemoryCache memoryCache, TimeProvider timeProvider) : ICacheService
{
    private static readonly ConcurrentDictionary<string, SemaphoreSlim> LazySlidingCacheLocks = new();
    
    public async Task<T> GetLazySlidingCacheValue<T>(LazySlideCacheOptions<T> options)
    {
        if (options.AbsoluteExpiration <= options.SlideThreshold)
        {
            throw new ArgumentException("Configuration not supported.");
        }
        
        var utcNow = timeProvider.GetUtcNow();
        
        var lazySlideCacheLock =
            LazySlidingCacheLocks.GetOrAdd(options.CacheKey, _ => new SemaphoreSlim(1, 1));
        
        //we want to wait until the lock is free before continuing
        await lazySlideCacheLock.WaitAsync();

        var slidePerformed = false;

        try
        {
            if (memoryCache.TryGetValue<LazySlideCacheObject>(options.CacheKey, out var lazySlideCacheObject))
            {
                ArgumentNullException.ThrowIfNull(lazySlideCacheObject);
            
                //check if we want to update the existing cache in the background lazily...
                if (lazySlideCacheObject.DateTimeUpdated.Add(options.SlideThreshold) < utcNow)
                {
                    //Sliding cache functionality
                    
                    //Update the cache value so the NEXT request gets a newer version of the latest expensive value fetch
                    //This approach means the cache entry is never guaranteed to be the exact latest value (unless a cache value does not exist) - but it is recent enough to not have a big impact
                    //The performance gain is a sufficient benefit to the value being potentially slightly behind the latest value
                    _ = SlideCache(options, lazySlideCacheLock, (T)lazySlideCacheObject.Value, utcNow);
                    slidePerformed = true;
                }
            
                //return the current cached value regardless of whether sliding was invoked
                return (T)lazySlideCacheObject.Value;
            }
            
            var value = await options.UpdateOperation();
            memoryCache.Set(options.CacheKey, new LazySlideCacheObject(value, utcNow),
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

    private async Task SlideCache<T>(LazySlideCacheOptions<T> options, SemaphoreSlim lazySlideCacheLock, T lazyValue, DateTimeOffset dateTime)
    {
        try
        {
            //update the cache datetime prematurely so that concurrent waiting threads do not trigger their own slide operation
            memoryCache.Set(options.CacheKey, new LazySlideCacheObject(lazyValue, dateTime),
                dateTime.Add(options.AbsoluteExpiration));
        }
        finally
        {
            //can release other threads now that the cache time has been updated
            lazySlideCacheLock.Release();
        }
        
        //then update the actual value now that no locks are being held
        var value = await options.UpdateOperation();
        memoryCache.Set(options.CacheKey, new LazySlideCacheObject(value, dateTime),
            dateTime.Add(options.AbsoluteExpiration));
    }

    internal record LazySlideCacheObject(object Value, DateTimeOffset DateTimeUpdated);
}
