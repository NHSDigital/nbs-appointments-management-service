using Microsoft.Extensions.Caching.Memory;

namespace Nhs.Appointments.Core;
public class ClinicalServiceProvider(IClinicalServiceStore store, IMemoryCache memoryCache) : IClinicalServiceProvider
{
    public async Task<string> GetServiceType(string service)
    {
        return await GetClinicalServiceProperty(service, x => x.ServiceType);
    }

    public async Task<string> GetServiceUrl(string service)
    {
        return await GetClinicalServiceProperty(service, x => x.Url);
    }

    public async Task<IEnumerable<ClinicalServiceType>> Get()
    {
        var clinicalServices = await store.Get();

        return clinicalServices;
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

    private async Task<string> GetClinicalServiceProperty(string service, Func<ClinicalServiceType, string> selector)
    {
        var clinicalServices = await Get();

        var matchedService = clinicalServices.FirstOrDefault(x => x.Value == service);
        return matchedService != null ? selector(matchedService) : null;
    }
}
