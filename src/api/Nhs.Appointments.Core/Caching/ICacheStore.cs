namespace Nhs.Appointments.Core.Caching;

public interface ICacheStore
{
    Task<T> GetAsync<T>(string key);
    Task<bool> TryGetAsync<T>(string key, out T value);
    Task SetAsync<T>(string key, T value, DateTimeOffset absoluteExpiration);
    Task SetAsync<T>(string key, T value, TimeSpan absoluteExpirationRelativeToNow);
}
