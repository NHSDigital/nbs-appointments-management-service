using Microsoft.Extensions.Caching.Memory;

namespace Nhs.Appointments.Core;
public class ClinicalServiceProvider(IClinicalServiceStore store, IMemoryCache memoryCache) : IClinicalServiceProvider
{
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
        var cacheKey = "clinical-service";
        var clinicalServices = memoryCache.Get<IEnumerable<ClinicalServiceType>>(cacheKey);
        if (clinicalServices == null)
        {
            clinicalServices = await Get();
            memoryCache.Set(cacheKey, clinicalServices, DateTimeOffset.UtcNow.AddMinutes(60));
        }

        return clinicalServices;
    }
}
