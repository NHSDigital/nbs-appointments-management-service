using Nhs.Appointments.Core.Reports;
using Nhs.Appointments.Persistance.Models;

namespace Nhs.Appointments.Persistance;

public class AggregationStore(ITypedDocumentCosmosStore<AggregationDocument> store) : IAggregationStore
{
    private ITypedDocumentCosmosStore<AggregationDocument> Store { get; } = store;

    public async Task<DateTime?> GetLastRunDate(string id)
    {
        var document = await Store.GetDocument<AggregationDocument>(id);

        return document?.LastTriggeredUtcDate;
    }

    public async Task GetLastRunDate(string id, DateTime lastRunDateUtc)
    {
        var document = new AggregationDocument() { Id = id, LastTriggeredUtcDate = lastRunDateUtc };

        await Store.WriteAsync(document);
    }
}
