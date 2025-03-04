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
using Microsoft.FeatureManagement;
using Microsoft.FeatureManagement.FeatureFilters;
using Nhs.Appointments.Api.Models;
using Nhs.Appointments.Core;

namespace Nhs.Appointments.Api.Functions;

public class GetFeatureFlagByUserFunction(
    IFeatureManager featureManager,
    IValidator<EmptyRequest> validator,
    IUserContextProvider userContextProvider,
    ILogger<GetFeatureFlagByUserFunction> logger,
    IMetricsRecorder metricsRecorder)
    : BaseApiFunction<EmptyRequest, bool>(validator, userContextProvider, logger, metricsRecorder)
{
    [OpenApiOperation(operationId: "GetFeatureFlagByUser", tags: ["FeatureFlag"],
        Summary = "Get the enabled state for the requested flag, by user filter")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, "application/json", typeof(bool),
        Description = "Information for a single site")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.BadRequest, "application/json",
        typeof(IEnumerable<ErrorMessageResponseItem>), Description = "The body of the request is invalid")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, "application/json",
        typeof(ErrorMessageResponseItem), Description = "Unauthorized request to a protected API")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.Forbidden, "application/json", typeof(ErrorMessageResponseItem),
        Description = "Request failed due to insufficient permissions")]
    [Function("GetFeatureFlagByUserFunction")]
    public override Task<IActionResult> RunAsync(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "feature-flag/user")] HttpRequest req)
    {
        return base.RunAsync(req);
    }

    protected override async Task<ApiResult<bool>> HandleRequest(EmptyRequest request, ILogger logger)
    {
        var context = new TargetingContext { UserId = "user1@example123.com" };
        var isFeatureEnabled = await featureManager.IsEnabledAsync(FeatureFlags.TestFeatureUsersEnabled, context);
        return ApiResult<bool>.Success(isFeatureEnabled);
    }

    protected override Task<IEnumerable<ErrorMessageResponseItem>> ValidateRequest(EmptyRequest request)
    {
        return Task.FromResult(Enumerable.Empty<ErrorMessageResponseItem>());
    }
}
