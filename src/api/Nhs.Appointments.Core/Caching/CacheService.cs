using System.Collections.Concurrent;
using Microsoft.Extensions.Caching.Memory;

namespace Nhs.Appointments.Core.Caching;

public record LazySlideCacheOptions<T>(string CacheKey, Func<Task<T>> UpdateOperation, TimeSpan SlideThreshold, TimeSpan AbsoluteExpiration);

public interface ICacheService
{
    Task<T> GetLazySlidingCacheValue<T>(LazySlideCacheOptions<T> options, bool isSlide = false);
}

public class CacheService(IMemoryCache memoryCache, TimeProvider timeProvider) : ICacheService
{
    private static readonly ConcurrentDictionary<string, SemaphoreSlim> LazySlidingCacheLocks = new();
    
    public async Task<T> GetLazySlidingCacheValue<T>(LazySlideCacheOptions<T> options, bool isSlide = false)
    {
        if (options.AbsoluteExpiration <= options.SlideThreshold)
        {
            throw new ArgumentException("Configuration not supported.");
        }
        
        var utcNow = timeProvider.GetUtcNow();
        var currentHoursAndMinutes = utcNow.DateTime;
        
        //TODO should this be created for both code paths??
        var lazySlideCacheLock =
            LazySlidingCacheLocks.GetOrAdd(options.CacheKey, _ => new SemaphoreSlim(1, 1));

        if (isSlide)
        {
            //a lock exists, and we are unwilling to wait any amount of time for it to be released
            //this means another request is already sliding this cache value and no work needs to be done
            if (!await lazySlideCacheLock.WaitAsync(0))
            {
                //lock never obtained, nothing to release??
               
                //no need to invoke the slide as another thread is performing the action concurrently
                return default;
            }

            try
            {
                //we got access immediately and want to do the work then release
                var value = await options.UpdateOperation();
                memoryCache.Set(options.CacheKey, new LazySlideCacheObject(value, currentHoursAndMinutes),
                    utcNow.Add(options.AbsoluteExpiration));
                return value;
            }
            finally
            {
                lazySlideCacheLock.Release();
            }
        }
        
        //we want to wait until the lock is free before continuing
        await lazySlideCacheLock.WaitAsync();

        try
        {
            if (memoryCache.TryGetValue<LazySlideCacheObject>(options.CacheKey, out var lazySlideCacheObject))
            {
                ArgumentNullException.ThrowIfNull(lazySlideCacheObject);
            
                //check if we want to update the existing cache in the background lazily...
                if (lazySlideCacheObject.DateTimeUpdated.Add(options.SlideThreshold) < currentHoursAndMinutes)
                {
                    //Sliding cache functionality

                    //Update the cache value so the NEXT request gets a newer version of the latest expensive value fetch
                    //This approach means the cache entry is never guaranteed to be the exact latest value (unless a cache value does not exist) - but it is recent enough to not have a big impact
                    //The performance gain is a sufficient benefit to the value being potentially slightly behind the latest operavalue
                    _ = GetLazySlidingCacheValue(options, true);
                }
            
                //return the current cached value regardless of whether sliding was invoked
                return (T)lazySlideCacheObject.Value;
            }
            
            var value = await options.UpdateOperation();
            memoryCache.Set(options.CacheKey, new LazySlideCacheObject(value, currentHoursAndMinutes),
                utcNow.Add(options.AbsoluteExpiration));
            return value;
        }
        finally
        {
            lazySlideCacheLock.Release();
        }
    }
    
    // //TODO just use UTC datetime???
    // //use a minimal version of TimeOnly to keep the cache object as small as possible
    // private static TimeOnly HourAndMinutes(DateTime dateTime)
    // {
    //     return new TimeOnly(dateTime.Hour, dateTime.Minute);
    // }

    internal record LazySlideCacheObject(object Value, DateTime DateTimeUpdated);
}
