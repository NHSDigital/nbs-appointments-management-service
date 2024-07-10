namespace Nhs.Appointments.Core
{

    public class ApiClientService : IApiClientService
    {
        private readonly IApiClientProfileStore _profileStore;

        public ApiClientService(IApiClientProfileStore profileStore)
        {
            _profileStore = profileStore;
        }

        public Task<ApiClientProfile> Get(string clientId) => _profileStore.GetAsync(clientId);
    }

}
