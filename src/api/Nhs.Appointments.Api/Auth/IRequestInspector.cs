using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker.Http;


namespace Nhs.Appointments.Api.Auth;

public interface IRequestInspector
{
    Task<string> GetSiteId(HttpRequestData httpRequest);
}
