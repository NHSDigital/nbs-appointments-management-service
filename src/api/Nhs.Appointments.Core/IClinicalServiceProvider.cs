namespace Nhs.Appointments.Core;
public interface IClinicalServiceProvider
{
    Task<IEnumerable<ClinicalServiceType>> Get();
    Task<IEnumerable<ClinicalServiceType>> GetFromCache();
    Task<string> GetVaccineType(string service);
    Task<string> GetServiceUrl(string service);
}
