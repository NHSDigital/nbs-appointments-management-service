using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker.Http;


namespace Nhs.Appointments.Api.Auth;

public class NoSiteRequestInspector : IRequestInspector
{
    public Task<string> GetSiteId(HttpRequestData httpRequest)
    {
        return Task.FromResult(string.Empty);
    }
}
