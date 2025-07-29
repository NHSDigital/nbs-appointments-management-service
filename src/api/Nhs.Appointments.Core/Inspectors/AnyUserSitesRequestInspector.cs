using Microsoft.Azure.Functions.Worker.Http;

namespace Nhs.Appointments.Core.Inspectors;


public class AnyUserSitesRequestInspector : IRequestInspector
{
    // Resolve the sites from the User Role Assignment and not from the scope of the request
    public Task<IEnumerable<string>> GetSiteIds(HttpRequestData _)
    {
        throw new NotImplementedException();
    }
}
