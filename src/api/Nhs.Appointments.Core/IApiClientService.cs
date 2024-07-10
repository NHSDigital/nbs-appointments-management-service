namespace Nhs.Appointments.Core
{
    public interface IApiClientService
    {
        Task<ApiClientProfile> Get(string clientId);
    }

}
