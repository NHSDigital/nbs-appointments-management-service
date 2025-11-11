using FluentValidation;
using Microsoft.AspNetCore.Http;
using Nhs.Appointments.Api.Auth;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Nhs.Appointments.Api.Models;
using System.Collections.Generic;
using Nhs.Appointments.Core.Users;
using Nhs.Appointments.Core.Helpers;

namespace Nhs.Appointments.Api.Functions.HttpFunctions;

public abstract class SiteBasedResourceFunction<TResponse>(IValidator<SiteBasedResourceRequest> validator, IUserContextProvider userContextProvider, ILogger logger, IMetricsRecorder metricsRecorder) : BaseApiFunction<SiteBasedResourceRequest, TResponse>(validator, userContextProvider, logger, metricsRecorder)
{
    protected override Task<(IReadOnlyCollection<ErrorMessageResponseItem> errors, SiteBasedResourceRequest request)> ReadRequestAsync(HttpRequest req)
    {
        var requestedScope = req.Query.Keys.Contains("scope") ? req.Query["scope"].ToString() : "*";

        if (req.Query.Keys.Contains("site"))
        {
            var site = req.Query["site"];
            return Task.FromResult((ErrorMessageResponseItem.None, new SiteBasedResourceRequest(site, requestedScope)));
        }

        var siteId = RestUriHelper.GetResourceIdFromPath(req.Path.ToUriComponent(), "sites");
        return Task.FromResult((ErrorMessageResponseItem.None, new SiteBasedResourceRequest(siteId, requestedScope)));
    }
}
