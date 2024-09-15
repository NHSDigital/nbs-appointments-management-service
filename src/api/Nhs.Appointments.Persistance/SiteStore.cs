using AutoMapper;
using Microsoft.Azure.Cosmos;
using Nhs.Appointments.Core;
using Nhs.Appointments.Persistance.Models;

namespace Nhs.Appointments.Persistance;

public class SiteStore(ITypedDocumentCosmosStore<SiteDocument> cosmosStore, IMapper mapper) : ISiteStore
{
    public Task<IEnumerable<SiteWithDistance>> GetSitesByArea(double longitude, double latitude, int searchRadius)
    {
        var query = new QueryDefinition(
                query: "SELECT site, ROUND(ST_DISTANCE(site.location, {\"type\": \"Point\", \"coordinates\":[@longitude, @latitude]})) as distance " + 
                       "FROM index_data site " + 
                       "WHERE ST_DISTANCE(site.location, {\"type\": \"Point\", \"coordinates\":[@longitude, @latitude]}) < @searchRadius")
            .WithParameter("@longitude", longitude)
            .WithParameter("@latitude", latitude)
            .WithParameter("@searchRadius", searchRadius);
        return cosmosStore.RunSqlQueryAsync<SiteWithDistance>(query);
    }
    
    public Task<Site> GetSiteById(string siteId)
    {
        return GetOrDefault(siteId);
    }
    
    public async Task<bool> UpdateSiteAttributes(string siteId, IEnumerable<AttributeValue> attributeValues)
    {
        var originalDocument = await GetOrDefault(siteId);
        if (originalDocument == null)
        {
            return false;
        }
        var documentType = cosmosStore.GetDocumentType();
        var siteDocumentPatch = PatchOperation.Add("/attributeValues", attributeValues);
        await cosmosStore.PatchDocument(documentType, siteId, siteDocumentPatch);
        return true;
    }
    
    private async Task<Site> GetOrDefault(string siteId)
    {
        try
        {
            return await cosmosStore.GetDocument<Site>(siteId);
        }
        catch (CosmosException ex) when ( ex.StatusCode == System.Net.HttpStatusCode.NotFound )
        {
            return default;
        }
    }
    
}