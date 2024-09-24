namespace Nhs.Appointments.Core;

public interface IAttributeDefinitionsStore
{
    Task<IEnumerable<AttributeDefinition>> GetAttributeDefinitionsDocument();
}
