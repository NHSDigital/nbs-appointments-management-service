namespace Nhs.Appointments.Core.Caching;

public record CacheOptions<T>(Func<Task<T>> UpdateOperation, TimeSpan AbsoluteExpiration);
