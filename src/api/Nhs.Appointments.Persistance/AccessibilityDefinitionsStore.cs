using Nhs.Appointments.Core.Sites;
using Nhs.Appointments.Persistance.Models;

namespace Nhs.Appointments.Persistance;

public class AccessibilityDefinitionsStore(
    ITypedDocumentCosmosStore<AccessibilityDefinitionsDocument> cosmosStore,
    IMetricsRecorder metricsRecorder) : IAccessibilityDefinitionsStore
{
    private const string AccessibilitySetsDocumentId = "accessibilities";

    public async Task<IEnumerable<AccessibilityDefinition>> GetAccessibilityDefinitionsDocument()
    {
        using (metricsRecorder.BeginScope(MetricScopes.AccessibilityDefinitions.Get))
        {
            var document = await cosmosStore.GetDocument<AccessibilityDefinitionsDocument>(AccessibilitySetsDocumentId);

            return document.AccessibilityDefinitions;
        }
    }
}
