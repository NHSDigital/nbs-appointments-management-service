using Microsoft.Azure.Cosmos;
using Nhs.Appointments.Core.Reports;
using Nhs.Appointments.Persistance.Models;

namespace Nhs.Appointments.Persistance;

public class AggregationStore(ITypedDocumentCosmosStore<AggregationDocument> store) : IAggregationStore
{
    private const string AggregationDocumentId = "daily-site-summary";
    private ITypedDocumentCosmosStore<AggregationDocument> Store { get; } = store;

    public async Task<DateTimeOffset?> GetLastRunDate()
    {
        var document = await Store.GetDocument<AggregationDocument>(AggregationDocumentId);

        return document?.LastTriggeredUtcDate;
    }

    public async Task SetLastRunDate(DateTimeOffset lastTriggerUtcDate)
    {
        var document = await Store.GetDocument<AggregationDocument>(AggregationDocumentId);

        if (document is null)
        {
            await Store.PatchDocument(Store.GetDocumentType(), AggregationDocumentId,
                PatchOperation.Set("/lastTriggerUtcDate", lastTriggerUtcDate));
            return;
        }

        await Store.WriteAsync(new AggregationDocument { Id = AggregationDocumentId, LastTriggeredUtcDate = lastTriggerUtcDate });
    }
}
