using System.Linq.Expressions;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using Xunit.Gherkin.Quick;

namespace Nhs.Appointments.Jobs.Aggregator.Integration;

public abstract partial class BaseFeatureSteps : Feature
{
    
    
    protected static async Task<IEnumerable<TDocument>> RunQueryAsync<TDocument>(Container container,
        Expression<Func<TDocument, bool>> predicate)
    {
        var queryFeed = container.GetItemLinqQueryable<TDocument>().Where(predicate).ToFeedIterator();
        var results = new List<TDocument>();
        using (queryFeed)
        {
            while (queryFeed.HasMoreResults)
            {
                var resultSet = await queryFeed.ReadNextAsync();
                results.AddRange(resultSet);
            }
        }

        return results;
    }
}
