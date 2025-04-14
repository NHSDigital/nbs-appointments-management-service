namespace Nhs.Appointments.Core
{
    public interface IServiceTypeStore
    {
        Task<IReadOnlyCollection<ServiceType>> Get();
    }
}
