using Nhs.Appointments.Core;
using Nhs.Appointments.Persistance.Models;

namespace Nhs.Appointments.Persistance;

public class AccessibilityDefinitionsStore(ITypedDocumentCosmosStore<AccessibilityDefinitionsDocument> cosmosStore) : IAccessibilityDefinitionsStore
{
    private const string AttributeSetsDocumentId = "attributes";
    public async Task<IEnumerable<AccessibilityDefinition>> GetAccessibilityDefinitionsDocument()
    {
        var document = await cosmosStore.GetDocument<AccessibilityDefinitionsDocument>(AttributeSetsDocumentId);
        return document.AccessibilityDefinitions;
    }
}