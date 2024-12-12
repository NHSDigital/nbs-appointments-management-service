using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker.Http;


namespace Nhs.Appointments.Api.Auth;

public interface IRequestInspector
{
    Task<IEnumerable<string>> GetSiteIds(HttpRequestData httpRequest);
}
