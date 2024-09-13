namespace Nhs.Appointments.Core;

public class AttributeDefinitionsService(IAttributeDefinitionsStore attributeDefinitionsStore) : IAttributeDefinitionsService
{
    public Task<IEnumerable<AttributeDefinition>> GetAttributeDefinitions()
    {
        return attributeDefinitionsStore.GetAttributeDefinitionsDocument();
    }
}