using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker.Http;
using Nhs.Appointments.Core;


namespace Nhs.Appointments.Api.Auth;

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
