namespace Nhs.Appointments.Core.Caching;

internal record LazySlideCacheObject(object Value, DateTimeOffset DateTimeUpdated);
internal record CacheObject<T>(T Value);
