using Nhs.Appointments.Core.Caching;

namespace Nhs.Appointments.Core.ClinicalServices;
public class ClinicalServiceProvider(IClinicalServiceStore store, ICacheService cacheService) : IClinicalServiceProvider
{
    private readonly TimeSpan _cacheDuration = TimeSpan.FromMinutes(60);
    
    public async Task<IEnumerable<ClinicalServiceType>> Get()
    {
        var clinicalServices = await store.Get();

        return clinicalServices;
    }

    public async Task<ClinicalServiceType> Get(string service)
    {
        var clinicalServices = await store.Get();

        return clinicalServices.FirstOrDefault(x => x.Value == service);
    }

    public async Task<IEnumerable<ClinicalServiceType>> GetFromCache()
    {
        return await cacheService.GetCacheValue(
            CacheKeys.ClinicalService, 
            new CacheOptions<IEnumerable<ClinicalServiceType>>(
                Get, 
                _cacheDuration));
    }
}
