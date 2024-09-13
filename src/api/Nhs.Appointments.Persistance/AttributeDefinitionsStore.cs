using Nhs.Appointments.Core;
using Nhs.Appointments.Persistance.Models;

namespace Nhs.Appointments.Persistance;

public class AttributeDefinitionsStore(ITypedDocumentCosmosStore<AttributeDefinitionsDocument> cosmosStore) : IAttributeDefinitionsStore
{
    private const string AttributeSetsDocumentId = "attributes";
    public async Task<IEnumerable<AttributeDefinition>> GetAttributeDefinitionsDocument()
    {
        var document = await cosmosStore.GetDocument<AttributeDefinitionsDocument>(AttributeSetsDocumentId);
        return document.AttributeDefinitions;
    }
}