namespace Nhs.Appointments.Core.Caching;

public record LazySlideCacheOptions<T>(Func<Task<T>> UpdateOperation, TimeSpan AbsoluteExpiration, TimeSpan SlideThreshold);
public record CacheOptions<T>(Func<Task<T>> UpdateOperation, TimeSpan AbsoluteExpiration);
