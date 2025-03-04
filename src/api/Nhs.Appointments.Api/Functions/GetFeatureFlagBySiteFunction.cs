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

public class GetFeatureFlagBySiteFunction(
    IFeatureManager featureManager,
    IValidator<SiteBasedResourceRequest> validator,
    IUserContextProvider userContextProvider,
    ILogger<GetFeatureFlagBySiteFunction> logger,
    IMetricsRecorder metricsRecorder)
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
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "feature-flag/site")] HttpRequest req)
    {
        return base.RunAsync(req);
    }

    protected override async Task<ApiResult<bool>> HandleRequest(SiteBasedResourceRequest request, ILogger logger)
    {
        var context = new TargetingContext { Groups = ["Site:5914b64a-66bb-4ee2-ab8a-94958c1fdfcb"] };
        var isFeatureEnabled = await featureManager.IsEnabledAsync(FeatureFlags.TestFeatureSitesEnabled, context);
        return ApiResult<bool>.Success(isFeatureEnabled);
    }

    protected override Task<IEnumerable<ErrorMessageResponseItem>> ValidateRequest(SiteBasedResourceRequest request)
    {
        return Task.FromResult(Enumerable.Empty<ErrorMessageResponseItem>());
    }
}
