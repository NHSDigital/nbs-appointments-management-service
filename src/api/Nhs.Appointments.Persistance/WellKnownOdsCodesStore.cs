using Nhs.Appointments.Core.OdsCodes;
using Nhs.Appointments.Persistance.Models;

namespace Nhs.Appointments.Persistance;

public class WellKnownOdsCodesStore(
    ITypedDocumentCosmosStore<WellKnownOdsCodesDocument> cosmosStore,
    IMetricsRecorder metricsRecorder
    ) : IWellKnownOdsCodesStore
{
    private const string WellKnownOdsCodeDocumentId = "well_known_ods_codes";

    public async Task<IEnumerable<WellKnownOdsEntry>> GetWellKnownOdsCodesDocument()
    {
        using (metricsRecorder.BeginScope(MetricScopes.WellKnownOdsCodes.Get))
        {
            var document = await cosmosStore.GetDocument<WellKnownOdsCodesDocument>(WellKnownOdsCodeDocumentId);
            return document.Entries; 
        }
    }
}
