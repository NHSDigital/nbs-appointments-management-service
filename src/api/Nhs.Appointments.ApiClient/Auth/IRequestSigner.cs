namespace Nhs.Appointments.ApiClient.Auth
{
    public interface IRequestSigner
    {
        Task SignRequestAsync(HttpRequestMessage request);
    }
}
