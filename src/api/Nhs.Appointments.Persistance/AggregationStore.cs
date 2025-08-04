using Microsoft.Azure.Cosmos;
using Nhs.Appointments.Core.Reports;
using Nhs.Appointments.Persistance.Models;

namespace Nhs.Appointments.Persistance;

public class AggregationStore(ITypedDocumentCosmosStore<AggregationDocument> store) : IAggregationStore
{
    private const string AggregationDocumentId = "daily-site-summary";
    private ITypedDocumentCosmosStore<AggregationDocument> Store { get; } = store;

    public async Task<Aggregation?> GetLastRun()
    {
        return await Store.GetByIdOrDefaultAsync<Aggregation>(AggregationDocumentId);
    }

    public async Task SetLastRun(DateTimeOffset lastTriggerUtcDate, DateOnly aggregationFrom, DateOnly aggregationTo, DateOnly dateUntilAggregated)
    {
        var document = await Store.GetByIdOrDefaultAsync<AggregationDocument>(AggregationDocumentId);

        if (document is not null)
        {
            await Store.PatchDocument(Store.GetDocumentType(), AggregationDocumentId,
                PatchOperation.Set("/lastTriggerUtcDate", lastTriggerUtcDate),
                PatchOperation.Set("/lastRunMetaData/fromDateOnly", aggregationFrom),
                PatchOperation.Set("/lastRunMetaData/toDateOnly", aggregationTo),
                PatchOperation.Set("/lastRunMetaData/lastRanToDateOnly", dateUntilAggregated));
            return;
        }

        await Store.WriteAsync(new AggregationDocument
        {
            Id = AggregationDocumentId, 
            LastTriggeredUtcDate = lastTriggerUtcDate, 
            DocumentType = Store.GetDocumentType(),
            LastRunMetaData = new AggregationLastRunMetaData()
            {
                FromDateOnly = aggregationFrom,
                ToDateOnly = aggregationTo,
                LastRanToDateOnly = dateUntilAggregated
            }
        });
    }
}
