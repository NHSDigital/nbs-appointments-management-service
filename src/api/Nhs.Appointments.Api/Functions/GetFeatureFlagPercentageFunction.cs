using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Extensions.Logging;
using Nhs.Appointments.Api.Features;
using Nhs.Appointments.Api.Models;
using Nhs.Appointments.Core;
using Nhs.Appointments.Core.Inspectors;

namespace Nhs.Appointments.Api.Functions;

public class GetFeatureFlagPercentageFunction(
    IValidator<EmptyRequest> validator,
    IUserContextProvider userContextProvider,
    ILogger<GetFeatureFlagPercentageFunction> logger,
    IMetricsRecorder metricsRecorder,
    IFeatureToggleHelper featureToggleHelper)
    : BaseApiFunction<EmptyRequest, bool>(validator, userContextProvider, logger, metricsRecorder)
{
    [OpenApiOperation(operationId: "GetFeatureFlagPercentage", tags: ["FeatureFlag"],
        Summary = "Get the enabled state for the requested flag, using the percentage filter")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, "application/json", typeof(bool),
        Description = "Information for a single site")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.BadRequest, "application/json",
        typeof(IEnumerable<ErrorMessageResponseItem>), Description = "The body of the request is invalid")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, "application/json",
        typeof(ErrorMessageResponseItem), Description = "Unauthorized request to a protected API")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.Forbidden, "application/json", typeof(ErrorMessageResponseItem),
        Description = "Request failed due to insufficient permissions")]
    [Function("GetFeatureFlagPercentageFunction")]
    public override Task<IActionResult> RunAsync(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "feature-flag/percentage")]
        HttpRequest req, FunctionContext functionContext)
    {
        return base.RunAsync(req, functionContext);
    }

    protected override async Task<ApiResult<bool>> HandleRequest(EmptyRequest request, ILogger logger,
        FunctionContext functionContext)
    {
        var isFeatureEnabled = await featureToggleHelper.IsFeatureEnabledForFunction(Flags.TestFeaturePercentageEnabled, functionContext, Principal, new NoSiteRequestInspector());
        return ApiResult<bool>.Success(isFeatureEnabled);
    }

    protected override Task<IEnumerable<ErrorMessageResponseItem>> ValidateRequest(EmptyRequest request)
    {
        return Task.FromResult(Enumerable.Empty<ErrorMessageResponseItem>());
    }
}
