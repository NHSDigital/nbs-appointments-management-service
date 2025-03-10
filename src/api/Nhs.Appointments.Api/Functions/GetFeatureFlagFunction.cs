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
    private FunctionContext FunctionContext { get; set; }
    
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
        HttpRequest req,
        FunctionContext functionContext)
    {
        FunctionContext = functionContext;
        return base.RunAsync(req, functionContext);
    }

    protected override async Task<ApiResult<bool>> HandleRequest(FeatureFlagEnabledRequest enabledRequest,
        ILogger logger,
        FunctionContext functionContext)
    {
        //fallback to the API user if not provided
        var userId = enabledRequest.UserOverrideId.IsNullOrWhiteSpace()
            ? Principal.Claims.GetUserEmail()
            : enabledRequest.UserOverrideId;

        string[] siteIds = null;

        if (!enabledRequest.SiteId.IsNullOrWhiteSpace())
        {
            siteIds = [enabledRequest.SiteId];
        }

        var isFeatureEnabled = await featureToggleHelper.IsFeatureEnabled(
            enabledRequest.Flag,
            userId,
            siteIds);

        return ApiResult<bool>.Success(isFeatureEnabled);
    }

    protected override async
        Task<(IReadOnlyCollection<ErrorMessageResponseItem> errors, FeatureFlagEnabledRequest request)>
        ReadRequestAsync(HttpRequest req)
    {
        var flag = req.HttpContext.GetRouteValue("flag")?.ToString();
        var siteId = req.Query["siteId"].ToString();
        var userId = req.Query["userId"].ToString();

        return ([], new FeatureFlagEnabledRequest(flag, siteId, userId));
    }
}
