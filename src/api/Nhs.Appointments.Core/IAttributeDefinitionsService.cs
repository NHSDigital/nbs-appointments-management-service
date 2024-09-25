namespace Nhs.Appointments.Core;

public interface IAttributeDefinitionsService
{
    Task<IEnumerable<AttributeDefinition>> GetAttributeDefinitions();
}
