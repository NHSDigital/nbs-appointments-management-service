using System.Linq.Expressions;
using BookingsDataExtracts.Documents;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using Microsoft.Extensions.Options;
using Nhs.Appointments.Persistance.Models;

namespace BookingsDataExtracts;

public class CosmosStore<TDocument>(CosmosClient cosmosClient, IOptions<CosmosStoreOptions> options)
{
    public Task<IEnumerable<TModel>> RunQueryAsync<TModel>(Expression<Func<TDocument, bool>> predicate, Expression<Func<TDocument, TModel>> projection)
    {
        var queryFeed = GetContainer().GetItemLinqQueryable<TDocument>().Where(predicate).ToFeedIterator();
        return IterateResults(queryFeed, projection.Compile());
    }

    public Task<IEnumerable<TModel>> RunSqlQueryAsync<TModel>(QueryDefinition query)
    {
        var queryFeed = GetContainer().GetItemQueryIterator<TModel>(
            queryDefinition: query);

        return IterateResults(queryFeed, item => item);
    }

    private async Task<IEnumerable<TOutput>> IterateResults<TSource, TOutput>(FeedIterator<TSource> queryFeed, Func<TSource, TOutput> map)
    {
        var requestCharge = 0.0;
        var results = new List<TOutput>();
        using (queryFeed)
        {
            while (queryFeed.HasMoreResults)
            {
                var resultSet = await queryFeed.ReadNextAsync();
                results.AddRange(resultSet.Select(map));
                requestCharge += resultSet.RequestCharge;
            }
        }
        return results;
    }

    private string GetContainerName() => typeof(TDocument).Name switch
    {
        nameof(NbsBookingDocument) => "booking_data",
        nameof(SiteDocument) => "core_data",
        _ => throw new NotSupportedException()
    };

    protected Container GetContainer() => cosmosClient.GetContainer(options.Value.DatabaseName, GetContainerName());
}

public class CosmosStoreOptions
{
    public string DatabaseName { get; set; }
}
