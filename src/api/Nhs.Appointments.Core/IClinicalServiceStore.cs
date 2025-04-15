namespace Nhs.Appointments.Core
{
    public interface IClinicalServiceStore
    {
        Task<IReadOnlyCollection<ClinicalServiceType>> Get();
    }
}
