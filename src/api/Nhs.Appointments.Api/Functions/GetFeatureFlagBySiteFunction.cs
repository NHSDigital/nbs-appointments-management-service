using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Extensions.Logging;
using Nhs.Appointments.Api.Features;
using Nhs.Appointments.Api.Models;
using Nhs.Appointments.Core;
using Nhs.Appointments.Core.Inspectors;

namespace Nhs.Appointments.Api.Functions;

public class GetFeatureFlagBySiteFunction(
    IValidator<SiteBasedResourceRequest> validator,
    IUserContextProvider userContextProvider,
    ILogger<GetFeatureFlagBySiteFunction> logger,
    IMetricsRecorder metricsRecorder,
    IFeatureToggleHelper featureToggleHelper)
    : BaseApiFunction<SiteBasedResourceRequest, bool>(validator, userContextProvider, logger, metricsRecorder)
{
    [OpenApiOperation(operationId: "GetFeatureFlagBySite", tags: ["FeatureFlag"],
        Summary = "Get the enabled state for the requested flag, by site filter")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, "application/json", typeof(bool),
        Description = "Information for a single site")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.BadRequest, "application/json",
        typeof(IEnumerable<ErrorMessageResponseItem>), Description = "The body of the request is invalid")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, "application/json",
        typeof(ErrorMessageResponseItem), Description = "Unauthorized request to a protected API")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.Forbidden, "application/json", typeof(ErrorMessageResponseItem),
        Description = "Request failed due to insufficient permissions")]
    [Function("GetFeatureFlagBySiteFunction")]
    public override Task<IActionResult> RunAsync(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "feature-flag/sites/{site}")] HttpRequest req,
        FunctionContext functionContext)
    {
        return base.RunAsync(req, functionContext);
    }

    protected override async Task<ApiResult<bool>> HandleRequest(SiteBasedResourceRequest request, ILogger logger,
        FunctionContext functionContext)
    {
        var isFeatureEnabled = await featureToggleHelper.IsFeatureEnabledForFunction(
            Flags.TestFeatureSitesEnabled, functionContext, Principal, new SiteFromPathInspector());
        return ApiResult<bool>.Success(isFeatureEnabled);
    }

    protected override async
        Task<(IReadOnlyCollection<ErrorMessageResponseItem> errors, SiteBasedResourceRequest request)> ReadRequestAsync(
            HttpRequest req)
    {
        var site = req.HttpContext.GetRouteValue("site")?.ToString();
        return ([], new SiteBasedResourceRequest(site, "*"));
    }
}
