namespace Nhs.Appointments.Core
{
    public interface IClinicalService
    {
        Task<IEnumerable<ClinicalServiceType>> Get();
    }
}
