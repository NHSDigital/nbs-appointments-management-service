namespace Nhs.Appointments.Core
{
    public interface IClinicalServiceStore
    {
        Task<IEnumerable<ClinicalServiceType>> Get();
    }
}
