using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker.Http;


namespace Nhs.Appointments.Api.Auth;

public class SiteFromQueryStringInspector : IRequestInspector
{
    public Task<string> GetSiteId(HttpRequestData httpRequest)
    {
        if (httpRequest.Query.AllKeys.Contains("site"))
        {
            return Task.FromResult(httpRequest.Query.Get("site"));
        }

        return Task.FromResult(string.Empty);
    }
}
