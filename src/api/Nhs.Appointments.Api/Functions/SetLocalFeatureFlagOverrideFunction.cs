using System;
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
using Nhs.Appointments.Api.Models;
using Nhs.Appointments.Core;
using Nhs.Appointments.Core.Features;

namespace Nhs.Appointments.Api.Functions;

public class SetLocalFeatureFlagOverrideFunction(
    IValidator<SetLocalFeatureFlagOverrideRequest> validator,
    IUserContextProvider userContextProvider,
    ILogger<SetLocalFeatureFlagOverrideFunction> logger,
    IMetricsRecorder metricsRecorder,
    IFeatureToggleHelper featureToggleHelper)
    : BaseApiFunction<SetLocalFeatureFlagOverrideRequest, EmptyResponse>(validator, userContextProvider, logger, metricsRecorder)
{
    [OpenApiOperation(operationId: "SetLocalFeatureFlagOverride", tags: ["FeatureFlag"],
        Summary = "Override the enabled state for the requested flag")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, "application/json", typeof(bool), Description = "")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.BadRequest, "application/json",
        typeof(IEnumerable<ErrorMessageResponseItem>), Description = "The body of the request is invalid")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, "application/json",
        typeof(ErrorMessageResponseItem), Description = "Unauthorized request to a protected API")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.Forbidden, "application/json", typeof(ErrorMessageResponseItem),
        Description = "Request failed due to insufficient permissions")]
    [Function("SetLocalFeatureFlagOverrideFunction")]
    [AllowAnonymous]
    public override Task<IActionResult> RunAsync(
        [HttpTrigger(AuthorizationLevel.Anonymous, "patch", Route = "feature-flag-override/{flag}")] HttpRequest req)
    {
        var appConfigEnvironment = Environment.GetEnvironmentVariable("APP_CONFIG_CONNECTION");

        if (appConfigEnvironment != "local")
        {
            return Task.FromResult(ProblemResponse(HttpStatusCode.Forbidden, "Overriding feature-flags is only supported locally"));
        }
        
        return base.RunAsync(req);
    }

    protected override Task<ApiResult<EmptyResponse>> HandleRequest(SetLocalFeatureFlagOverrideRequest request, ILogger logger)
    {
#pragma warning disable CS0618 // Intended use
        featureToggleHelper.SetOverride(request.Flag, request.Enabled);
#pragma warning restore CS0618 // Intended use
        return Task.FromResult(ApiResult<EmptyResponse>.Success(new EmptyResponse()));
    }

    protected override Task<(IReadOnlyCollection<ErrorMessageResponseItem> errors, SetLocalFeatureFlagOverrideRequest request)> ReadRequestAsync(HttpRequest req)
    {
        var flag = req.HttpContext.GetRouteValue("flag")?.ToString();
        var enabled = bool.Parse(req.Query["enabled"]);
        return Task.FromResult<(IReadOnlyCollection<ErrorMessageResponseItem> errors, SetLocalFeatureFlagOverrideRequest request)>(([], new SetLocalFeatureFlagOverrideRequest(flag, enabled)));
    }
}
