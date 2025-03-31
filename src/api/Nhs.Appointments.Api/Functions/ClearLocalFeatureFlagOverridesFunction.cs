using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Extensions.Logging;
using Nhs.Appointments.Api.Features;
using Nhs.Appointments.Api.Models;
using Nhs.Appointments.Core;

namespace Nhs.Appointments.Api.Functions;

public class ClearLocalFeatureFlagOverridesFunction(
    IValidator<EmptyRequest> validator,
    IUserContextProvider userContextProvider,
    ILogger<ClearLocalFeatureFlagOverridesFunction> logger,
    IMetricsRecorder metricsRecorder,
    IFeatureToggleHelper featureToggleHelper)
    : BaseApiFunction<EmptyRequest, EmptyResponse>(validator, userContextProvider, logger, metricsRecorder)
{
    [OpenApiOperation(operationId: "ClearLocalFeatureFlagOverrides", tags: ["FeatureFlag"],
        Summary = "Clear the local overrides for all feature flags")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, "application/json", typeof(bool), Description = "")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.BadRequest, "application/json",
        typeof(IEnumerable<ErrorMessageResponseItem>), Description = "The body of the request is invalid")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, "application/json",
        typeof(ErrorMessageResponseItem), Description = "Unauthorized request to a protected API")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.Forbidden, "application/json", typeof(ErrorMessageResponseItem),
        Description = "Request failed due to insufficient permissions")]
    [Function("ClearLocalFeatureFlagOverridesFunction")]
    [AllowAnonymous]
    public override Task<IActionResult> RunAsync([HttpTrigger(AuthorizationLevel.Anonymous, "patch", Route = "feature-flag-overrides-clear")] HttpRequest req)
    {
        var appConfigEnvironment = Environment.GetEnvironmentVariable("APP_CONFIG_CONNECTION");

        if (appConfigEnvironment != "local")
        {
            return Task.FromResult(ProblemResponse(HttpStatusCode.Forbidden, "Clearing overridden feature-flags is only supported locally"));
        }
        
        return base.RunAsync(req);
    }

    protected override Task<ApiResult<EmptyResponse>> HandleRequest(EmptyRequest request, ILogger logger)
    {
#pragma warning disable CS0618 // Intended use
        featureToggleHelper.ClearOverrides();
#pragma warning restore CS0618 // Intended use
        return Task.FromResult(ApiResult<EmptyResponse>.Success(new EmptyResponse()));
    }
    
    protected override Task<IEnumerable<ErrorMessageResponseItem>> ValidateRequest(EmptyRequest request)
    {
        return Task.FromResult(Enumerable.Empty<ErrorMessageResponseItem>());
    }
}
