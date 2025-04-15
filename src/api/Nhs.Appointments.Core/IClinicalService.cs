namespace Nhs.Appointments.Core
{
    public interface IClinicalService
    {
        Task<IReadOnlyCollection<ClinicalServiceType>> Get();
    }
}
