using Nhs.Appointments.Core.Eula;
using Nhs.Appointments.Persistance.Models;

namespace Nhs.Appointments.Persistance;

public class EulaStore(
    ITypedDocumentCosmosStore<EulaDocument> documentStore,
    IMetricsRecorder metricsRecorder) : IEulaStore
{
    public async Task<EulaVersion> GetLatestVersion()
    {
        using (metricsRecorder.BeginScope(MetricScopes.Eula.GetLatest))
        {
            return await documentStore.GetDocument<EulaVersion>("eula");
        }
    }
}
