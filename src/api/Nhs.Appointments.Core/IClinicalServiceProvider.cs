namespace Nhs.Appointments.Core;
public interface IClinicalServiceProvider
{
    Task<IEnumerable<ClinicalServiceType>> Get();
    Task<ClinicalServiceType> Get(string service);
    Task<IEnumerable<ClinicalServiceType>> GetFromCache();
}
