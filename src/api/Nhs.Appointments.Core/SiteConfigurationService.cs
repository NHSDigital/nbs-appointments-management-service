namespace Nhs.Appointments.Core;

public interface ISiteConfigurationService
{
    Task PutSiteConfigurationAsync(SiteConfiguration siteConfiguration);
    Task<SiteConfiguration> GetSiteConfigurationAsync(string siteUrn);
    Task<SiteConfiguration> GetSiteConfigurationOrDefaultAsync(string siteUrn);
}

public class SiteConfigurationService : ISiteConfigurationService
{
    private readonly ISiteConfigurationStore _store;

    public SiteConfigurationService(ISiteConfigurationStore store) 
    {
        _store = store;
    }

    public Task PutSiteConfigurationAsync(SiteConfiguration siteConfiguration)
    {        
        return _store.ReplaceOrCreate(siteConfiguration);
    }

    public Task<SiteConfiguration> GetSiteConfigurationAsync(string siteUrn)
    {
        return _store.GetAsync(siteUrn);
    }
    
    public Task<SiteConfiguration> GetSiteConfigurationOrDefaultAsync(string siteUrn)
    {
        return _store.GetOrDefault(siteUrn);
    }
}    
