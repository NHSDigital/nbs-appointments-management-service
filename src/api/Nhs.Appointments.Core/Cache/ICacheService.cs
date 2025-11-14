namespace Nhs.Appointments.Core.Cache;

public interface ICacheService<TModel>
{
    bool TryGetCache<TCacheModel>(string key, out TCacheModel model);
    
    Task SetCache<TCacheModel>(string key, TCacheModel model, DateTimeOffset expiration);
    
    Task RemoveCache(string key);
    
    Task PatchCache(string key, TModel patch, Predicate<TModel> predicate, DateTimeOffset expiration);
}
