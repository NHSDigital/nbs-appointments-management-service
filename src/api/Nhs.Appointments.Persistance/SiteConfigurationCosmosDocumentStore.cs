using Microsoft.Azure.Cosmos;
using Nhs.Appointments.Core;
using Nhs.Appointments.Persistance.Models;

namespace Nhs.Appointments.Persistance;

public class SiteConfigurationCosmosDocumentStore : ISiteConfigurationStore
{
    private readonly ITypedDocumentCosmosStore<SiteConfigurationDocument> _store;

    public SiteConfigurationCosmosDocumentStore(ITypedDocumentCosmosStore<SiteConfigurationDocument> store)
    {
        _store = store;
    }

    public Task AssignPrefix(string site, int prefix)
    {
        var updatePrefix = PatchOperation.Replace("/referenceNumberGroup", prefix);
        var partitionKey = _store.GetDocumentType();
        return _store.PatchDocument(partitionKey, site, updatePrefix);
    }

    public Task<SiteConfiguration> GetAsync(string site)
    {
        return _store.GetByIdAsync<SiteConfiguration>(site);
    }

    public async Task<SiteConfiguration> GetOrDefault(string site)
    {
        try
        {
            return await _store.GetByIdAsync<SiteConfiguration>(site);
        }
        catch (CosmosException ex) when ( ex.StatusCode == System.Net.HttpStatusCode.NotFound )
        {
            return default;
        }
    }    

    public async Task ReplaceOrCreate(SiteConfiguration siteConfiguration)
    {        
        var originalDocument = await GetOrDefault(siteConfiguration.Site);
        var siteReferenceGroup = originalDocument?.ReferenceNumberGroup ?? 0;
        siteConfiguration.ReferenceNumberGroup = siteReferenceGroup;
        await InsertAsync(siteConfiguration);
    }

    private Task InsertAsync(SiteConfiguration siteConfiguration)
    {
        var document = _store.ConvertToDocument(siteConfiguration);
        return _store.WriteAsync(document);
    }
}