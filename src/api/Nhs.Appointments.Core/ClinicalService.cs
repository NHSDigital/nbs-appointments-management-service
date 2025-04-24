
namespace Nhs.Appointments.Core
{
    public class ClinicalService(IClinicalServiceStore store) : IClinicalService
    {
        public async Task<IEnumerable<ClinicalServiceType>> Get() 
        {
            return await store.Get();
        }
    }
}
