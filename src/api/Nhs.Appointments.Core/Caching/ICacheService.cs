namespace Nhs.Appointments.Core.Caching;

public interface ICacheService
{
    Task<T> GetLazySlidingCacheValue<T>(string cacheKey, LazySlideCacheOptions<T> options);

    Task<T> GetCacheValue<T>(string cacheKey, CacheOptions<T> options);
}
