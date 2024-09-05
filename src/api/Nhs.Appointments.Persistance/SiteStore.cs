using AutoMapper;
using Microsoft.Azure.Cosmos;
using Nhs.Appointments.Core;
using Nhs.Appointments.Persistance.Models;

namespace Nhs.Appointments.Persistance;

public class SiteStore(ITypedDocumentCosmosStore<SiteDocument> cosmosStore, IMapper mapper) : ISiteStore
{
    public async Task<IEnumerable<Site>> GetSitesByArea(double longitude, double latitude, int searchRadius)
    {
        var query = new QueryDefinition(
                query: "SELECT * " + 
                       "FROM index_data site " + 
                       "WHERE ST_DISTANCE(site.location, {\"type\": \"Point\", \"coordinates\":[@longitude, @latitude]}) < @searchRadius")
                        .WithParameter("@longitude", longitude)
                        .WithParameter("@latitude", latitude)
                        .WithParameter("@searchRadius", searchRadius);
        var sites = await cosmosStore.RunSqlQueryAsync<SiteDocument>(query);
        
        return sites.Select(mapper.Map<Site>);
    }
}

