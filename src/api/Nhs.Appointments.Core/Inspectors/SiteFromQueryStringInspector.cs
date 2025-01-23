using Microsoft.Azure.Functions.Worker.Http;

namespace Nhs.Appointments.Core.Inspectors;

public class SiteFromQueryStringInspector : IRequestInspector
{
    public Task<IEnumerable<string>> GetSiteIds(HttpRequestData httpRequest)
    {
        if (httpRequest.Query.AllKeys.Contains("site"))
        {
            return Task.FromResult((IEnumerable<string>)[httpRequest.Query.Get("site")]);
        }

        return Task.FromResult(Enumerable.Empty<string>());
    }
}
