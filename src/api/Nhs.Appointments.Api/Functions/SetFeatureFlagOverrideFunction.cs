using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Extensions.Logging;
using Nhs.Appointments.Api.Features;
using Nhs.Appointments.Api.Models;
using Nhs.Appointments.Core;

namespace Nhs.Appointments.Api.Functions;

public class SetFeatureFlagOverrideFunction(
    IValidator<SetFeatureFlagOverrideRequest> validator,
    IUserContextProvider userContextProvider,
    ILogger<SetFeatureFlagOverrideFunction> logger,
    IMetricsRecorder metricsRecorder,
    IFeatureToggleHelper featureToggleHelper)
    : BaseApiFunction<SetFeatureFlagOverrideRequest, EmptyResponse>(validator, userContextProvider, logger, metricsRecorder)
{
    [OpenApiOperation(operationId: "SetFeatureFlagOverrideFunction", tags: ["FeatureFlag"],
        Summary = "Override the enabled state for the requested flag")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, "application/json", typeof(bool), Description = "")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.BadRequest, "application/json",
        typeof(IEnumerable<ErrorMessageResponseItem>), Description = "The body of the request is invalid")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, "application/json",
        typeof(ErrorMessageResponseItem), Description = "Unauthorized request to a protected API")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.Forbidden, "application/json", typeof(ErrorMessageResponseItem),
        Description = "Request failed due to insufficient permissions")]
    [Function("SetFeatureFlagOverrideFunction")]
    //TODO don't allow anon??
    [AllowAnonymous]
    public override Task<IActionResult> RunAsync(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "feature-flag-override/{flag}")] HttpRequest req)
    {
        return base.RunAsync(req);
    }

    protected override Task<ApiResult<EmptyResponse>> HandleRequest(SetFeatureFlagOverrideRequest request, ILogger logger)
    {
        featureToggleHelper.SetOverride(request.Flag, request.Enabled);
        return Task.FromResult(ApiResult<EmptyResponse>.Success(new EmptyResponse()));
    }

    protected override Task<(IReadOnlyCollection<ErrorMessageResponseItem> errors, SetFeatureFlagOverrideRequest request)> ReadRequestAsync(HttpRequest req)
    {
        var flag = req.HttpContext.GetRouteValue("flag")?.ToString();
        var enabled = bool.Parse(req.Query["enabled"]);
        return Task.FromResult<(IReadOnlyCollection<ErrorMessageResponseItem> errors, SetFeatureFlagOverrideRequest request)>(([], new SetFeatureFlagOverrideRequest(flag, enabled)));
    }
}
