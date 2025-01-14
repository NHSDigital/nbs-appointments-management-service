using Microsoft.Azure.Functions.Worker.Http;

namespace Nhs.Appointments.Core.Inspectors;

public interface IRequestInspector
{
    Task<IEnumerable<string>> GetSiteIds(HttpRequestData httpRequest);
}
