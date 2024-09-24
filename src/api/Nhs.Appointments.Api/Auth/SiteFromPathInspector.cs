using System;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker.Http;
using Nhs.Appointments.Core;


namespace Nhs.Appointments.Api.Auth;

public class SiteFromPathInspector : IRequestInspector
{
    public Task<string> GetSiteId(HttpRequestData httpRequest)
    {
        if (httpRequest.Url.AbsolutePath.Contains("sites"))
        {
            var siteId = RestUriHelper.GetResourceIdFromPath(httpRequest.Url.AbsolutePath, "sites");
            return Task.FromResult(siteId);
        }
        return Task.FromResult(string.Empty);
    }
}