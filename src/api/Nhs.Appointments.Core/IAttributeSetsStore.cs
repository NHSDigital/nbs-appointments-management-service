namespace Nhs.Appointments.Core;

public interface IAttributeSetsStore
{
    Task<AttributeSets> GetAttributeSets();
}
