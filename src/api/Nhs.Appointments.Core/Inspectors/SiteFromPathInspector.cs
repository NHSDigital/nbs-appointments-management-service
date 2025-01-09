using Microsoft.Azure.Functions.Worker.Http;

namespace Nhs.Appointments.Core.Inspectors;

public class SiteFromPathInspector : IRequestInspector
{
    public Task<IEnumerable<string>> GetSiteIds(HttpRequestData httpRequest)
    {
        if (httpRequest.Url.AbsolutePath.Contains("sites"))
        {
            var siteId = RestUriHelper.GetResourceIdFromPath(httpRequest.Url.AbsolutePath, "sites");
            return Task.FromResult((IEnumerable<string>)[siteId]);
        }
        return Task.FromResult(Enumerable.Empty<string>());
    }
}
