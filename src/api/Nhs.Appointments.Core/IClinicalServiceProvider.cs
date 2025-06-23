namespace Nhs.Appointments.Core;
public interface IClinicalServiceProvider
{
    Task<IEnumerable<ClinicalServiceType>> Get();
    Task<IEnumerable<ClinicalServiceType>> GetFromCache();
    Task<string> GetServiceType(string service);
    Task<string> GetServiceUrl(string service);
}
