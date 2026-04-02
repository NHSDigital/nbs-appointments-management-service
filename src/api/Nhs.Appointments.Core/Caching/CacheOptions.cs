namespace Nhs.Appointments.Core.Caching;

public record LazySlideCacheOptions<T>(Func<Task<T>> UpdateOperation, TimeSpan SlideThreshold, TimeSpan AbsoluteExpiration);
public record CacheOptions<T>(Func<Task<T>> UpdateOperation, TimeSpan AbsoluteExpiration, TryPatternOptions<T> TryPatternOptions = null);
public record TryPatternOptions<T>(bool UseTryPattern = true, T DefaultResponse = default);
