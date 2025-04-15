using Nhs.Appointments.Core.Features;

namespace Nhs.Appointments.Core
{
    public class ClinicalService(IClinicalServiceStore store, IFeatureToggleHelper featureToggleHelper) : IClinicalService
    {
        public async Task<IReadOnlyCollection<ClinicalServiceType>> Get() 
        {
            var services = await store.Get();

            var featureToggleTasks = services.Select(GetServiceFeatureStatus).ToList();

            await Task.WhenAll(featureToggleTasks);

            return featureToggleTasks.Where(task => task.Result.IsEnabled).Select(task => task.Result.Service).ToList();
        }

        private async Task<(ClinicalServiceType Service, bool IsEnabled)> GetServiceFeatureStatus(ClinicalServiceType service) 
        {
            return (service, await featureToggleHelper.IsFeatureEnabled(service.Value));
        }
    }
}
