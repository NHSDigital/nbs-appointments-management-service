using FluentValidation;
using Microsoft.AspNetCore.Http;
using Nhs.Appointments.Api.Auth;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Nhs.Appointments.Api.Models;
using Nhs.Appointments.Core;

namespace Nhs.Appointments.Api.Functions;

public abstract class SiteBasedResourceFunction<TResponse>(IValidator<SiteBasedResourceRequest> validator, IUserContextProvider userContextProvider, ILogger logger) : BaseApiFunction<SiteBasedResourceRequest, TResponse>(validator, userContextProvider, logger)
{
    protected override Task<(bool requestRead, SiteBasedResourceRequest request)> ReadRequestAsync(HttpRequest req)
    {
        if (req.Query.Keys.Contains("site"))
        {
            var site = req.Query["site"];
            return Task.FromResult<(bool requestRead, SiteBasedResourceRequest request)>((true, new SiteBasedResourceRequest(site, "*")));
        }

        var siteId = RestUriHelper.GetResourceIdFromPath(req.Path.ToUriComponent(), "sites");
        var requestedScope = req.Query["scope"].ToString();
        return Task.FromResult<(bool requestRead, SiteBasedResourceRequest request)>
            ((true, new SiteBasedResourceRequest(siteId, string.IsNullOrEmpty(requestedScope) ? "*" : requestedScope)));
    }
}
