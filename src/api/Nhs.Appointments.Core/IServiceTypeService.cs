namespace Nhs.Appointments.Core
{
    public interface IServiceTypeService
    {
        Task<IReadOnlyCollection<ServiceType>> Get();
    }
}
