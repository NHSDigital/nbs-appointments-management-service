using AutoMapper;
using Microsoft.Azure.Cosmos;
using Nhs.Appointments.Core;
using Nhs.Appointments.Persistance.Models;

namespace Nhs.Appointments.Persistance;

public class SiteStore(ITypedDocumentCosmosStore<SiteDocument> cosmosStore, IMapper mapper) : ISiteStore
{
    public async Task<IEnumerable<SiteWithDistance>> GetSitesByArea(double longitude, double latitude, int searchRadius)
    {
        var query = new QueryDefinition(
                query: "SELECT site, ROUND(ST_DISTANCE(site.location, {\"type\": \"Point\", \"coordinates\":[@longitude, @latitude]})) as distance " + 
                       "FROM index_data site " + 
                       "WHERE ST_DISTANCE(site.location, {\"type\": \"Point\", \"coordinates\":[@longitude, @latitude]}) < @searchRadius")
            .WithParameter("@longitude", longitude)
            .WithParameter("@latitude", latitude)
            .WithParameter("@searchRadius", searchRadius);
        var sites = await cosmosStore.RunSqlQueryAsync<SiteWithDistance>(query);
    
        return sites;
    }
    
    public async Task<Site> GetSiteById(string siteId)
    {
        try
        {
            var site = await cosmosStore.GetDocument<Site>(siteId);
            return site;
        }
        catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return default;
        }
    }
}