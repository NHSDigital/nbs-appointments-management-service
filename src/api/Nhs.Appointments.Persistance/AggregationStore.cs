using Microsoft.Azure.Cosmos;
using Nhs.Appointments.Core.Reports;
using Nhs.Appointments.Persistance.Models;

namespace Nhs.Appointments.Persistance;

public class AggregationStore(ITypedDocumentCosmosStore<AggregationDocument> store) : IAggregationStore
{
    private ITypedDocumentCosmosStore<AggregationDocument> Store { get; } = store;

    public async Task<DateTimeOffset?> GetLastRunDate(string id)
    {
        var document = await Store.GetDocument<AggregationDocument>(id);

        return document?.LastTriggeredUtcDate;
    }

    public async Task SetLastRunDate(string id, DateTimeOffset lastTriggerUtcDate)
    {
        var document = await Store.GetDocument<AggregationDocument>(id);

        if (document is null)
        {
            await Store.PatchDocument(Store.GetDocumentType(), id,
                PatchOperation.Set("/lastTriggerUtcDate", lastTriggerUtcDate));
            return;
        }

        await Store.WriteAsync(new AggregationDocument { Id = id, LastTriggeredUtcDate = lastTriggerUtcDate });
    }
}
