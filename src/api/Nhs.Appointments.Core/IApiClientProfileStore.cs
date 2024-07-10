namespace Nhs.Appointments.Core;

public interface IApiClientProfileStore
{
    Task<ApiClientProfile> GetAsync(string id);
}
