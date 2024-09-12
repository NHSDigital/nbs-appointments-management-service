namespace Nhs.Appointments.Core;

public class AttributeSetsService(IAttributeSetsStore attributeSetsStore) : IAttributeSetsService
{
    public Task<AttributeSets> GetAttributeSets()
    {
        return attributeSetsStore.GetAttributeSets();
    }
}