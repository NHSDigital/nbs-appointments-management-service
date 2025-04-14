using Nhs.Appointments.Core.Features;

namespace Nhs.Appointments.Core
{
    public class ServiceTypeService(IServiceTypeStore store, IFeatureToggleHelper featureToggleHelper) : IServiceTypeService
    {
        public async Task<IReadOnlyCollection<ServiceType>> Get() 
        {
            var services = await store.Get();

            var featureToggleTasks = services.Select(GetServiceFeatureStatus).ToList();

            await Task.WhenAll(featureToggleTasks);

            return featureToggleTasks.Where(task => task.Result.IsEnabled).Select(task => task.Result.Service).ToList();
        }

        private async Task<(ServiceType Service, bool IsEnabled)> GetServiceFeatureStatus(ServiceType service) 
        {
            return (service, await featureToggleHelper.IsFeatureEnabled(service.Value));
        }
    }
}
