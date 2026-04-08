namespace Nhs.Appointments.Core.Caching;

public interface ICacheStore
{
    Task<bool> TryGetAsync<T>(string key, out T value);
    Task SetAsync<T>(string key, T value, TimeSpan expirationRelativeToNow);
}
