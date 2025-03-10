using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Extensions;
using Microsoft.Extensions.Logging;
using Nhs.Appointments.Api.Features;
using Nhs.Appointments.Api.Models;
using Nhs.Appointments.Core;

namespace Nhs.Appointments.Api.Functions;

public class GetFeatureFlagFunction(
    IValidator<FeatureFlagEnabledRequest> validator,
    IUserContextProvider userContextProvider,
    ILogger<GetFeatureFlagFunction> logger,
    IMetricsRecorder metricsRecorder,
    IFeatureToggleHelper featureToggleHelper)
    : BaseApiFunction<FeatureFlagEnabledRequest, bool>(validator, userContextProvider, logger, metricsRecorder)
{
    [OpenApiOperation(operationId: "GetFeatureFlag", tags: ["FeatureFlag"],
        Summary =
            "Get the enabled state for the requested flag, by the siteId query parameter (if provided) and an optional userId override (if provided)")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, "application/json", typeof(bool),
        Description = "The enabled state for the requested flag")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.BadRequest, "application/json",
        typeof(IEnumerable<ErrorMessageResponseItem>), Description = "The body of the request is invalid")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, "application/json",
        typeof(ErrorMessageResponseItem), Description = "Unauthorized request to a protected API")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.Forbidden, "application/json", typeof(ErrorMessageResponseItem),
        Description = "Request failed due to insufficient permissions")]
    [Function("GetFeatureFlagFunction")]
    public override Task<IActionResult> RunAsync(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "feature-flag/{flag}")]
        HttpRequest req)
    {
        return base.RunAsync(req);
    }

    protected override async Task<ApiResult<bool>> HandleRequest(FeatureFlagEnabledRequest enabledRequest,
        ILogger logger)
    {
        //fallback to the API user if not provided
        var userId = enabledRequest.UserOverrideId.IsNullOrWhiteSpace()
            ? Principal.Claims.GetUserEmail()
            : enabledRequest.UserOverrideId;

        var isFeatureEnabled = await featureToggleHelper.IsFeatureEnabled(
            enabledRequest.Flag,
            userId,
            enabledRequest.SiteIds);

        return ApiResult<bool>.Success(isFeatureEnabled);
    }

    protected override async
        Task<(IReadOnlyCollection<ErrorMessageResponseItem> errors, FeatureFlagEnabledRequest request)>
        ReadRequestAsync(HttpRequest req)
    {
        var flag = req.HttpContext.GetRouteValue("flag")?.ToString();
        var siteIds = req.Query["siteId"].ToArray();
        var userId = req.Query["userId"].ToString();

        return ([], new FeatureFlagEnabledRequest(flag, siteIds, userId));
    }
}
