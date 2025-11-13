namespace Nhs.Appointments.Core.ClinicalServices
{
    public interface IClinicalServiceStore
    {
        Task<IEnumerable<ClinicalServiceType>> Get();
    }
}
