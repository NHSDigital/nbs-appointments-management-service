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
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace Nhs.Appointments.Api.Functions;

public class GetFeatureFlagFunction(
    IValidator<FeatureFlagRequest> validator,
    IUserContextProvider userContextProvider,
    ILogger<GetFeatureFlagFunction> logger,
    IMetricsRecorder metricsRecorder,
    IFeatureToggleHelper featureToggleHelper)
    : BaseApiFunction<FeatureFlagRequest, FeatureFlagResponse>(validator, userContextProvider, logger, metricsRecorder)
{
    [OpenApiOperation(operationId: "GetFeatureFlag", tags: ["FeatureFlag"],
        Summary = "Get the enabled state for the requested flag")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, "application/json", typeof(bool),
        Description = "The enabled state for the requested flag")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.BadRequest, "application/json",
        typeof(IEnumerable<ErrorMessageResponseItem>), Description = "The body of the request is invalid")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, "application/json",
        typeof(ErrorMessageResponseItem), Description = "Unauthorized request to a protected API")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.Forbidden, "application/json", typeof(ErrorMessageResponseItem),
        Description = "Request failed due to insufficient permissions")]
    [Function("GetFeatureFlagFunction")]
    [AllowAnonymous]
    public override Task<IActionResult> RunAsync(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "feature-flag/{flag}")] HttpRequest req)
    {
        return base.RunAsync(req);
    }

    protected override async Task<ApiResult<FeatureFlagResponse>> HandleRequest(FeatureFlagRequest request,
        ILogger logger)
    {
        var isFeatureEnabled = await featureToggleHelper.IsFeatureEnabled(request.Flag);
        return ApiResult<FeatureFlagResponse>.Success(new FeatureFlagResponse(isFeatureEnabled));
    }

    protected override Task<(IReadOnlyCollection<ErrorMessageResponseItem> errors, FeatureFlagRequest request)>
        ReadRequestAsync(HttpRequest req)
    {
        var flag = req.HttpContext.GetRouteValue("flag")?.ToString();
        return Task
            .FromResult<(IReadOnlyCollection<ErrorMessageResponseItem> errors, FeatureFlagRequest request)>(([],
                new FeatureFlagRequest(flag)));
    }
}
