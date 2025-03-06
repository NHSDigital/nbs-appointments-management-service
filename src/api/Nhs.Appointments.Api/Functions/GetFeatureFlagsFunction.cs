using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Extensions.Logging;
using Microsoft.FeatureManagement;
using Nhs.Appointments.Api.Features;
using Nhs.Appointments.Api.Models;
using Nhs.Appointments.Core;
using Nhs.Appointments.Core.Inspectors;

namespace Nhs.Appointments.Api.Functions;

public class GetFeatureFlagsFunction(
    IValidator<EmptyRequest> validator,
    IUserContextProvider userContextProvider,
    ILogger<GetFeatureFlagsFunction> logger,
    IMetricsRecorder metricsRecorder,
    IFunctionFeatureToggleHelper functionFeatureToggleHelper)
    : BaseApiFunction<EmptyRequest, GetFeatureFlagsResponse>(validator, userContextProvider, logger, metricsRecorder)
{
    [OpenApiOperation(operationId: "GetFeatureFlags", tags: ["FeatureFlag"],
        Summary = "Get the enabled state for all the defined feature flags")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, "application/json", typeof(GetFeatureFlagsResponse),
        Description = "Information for a single site")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.BadRequest, "application/json",
        typeof(IEnumerable<ErrorMessageResponseItem>), Description = "The body of the request is invalid")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, "application/json",
        typeof(ErrorMessageResponseItem), Description = "Unauthorized request to a protected API")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.Forbidden, "application/json", typeof(ErrorMessageResponseItem),
        Description = "Request failed due to insufficient permissions")]
    [Function("GetFeatureFlagsFunction")]
    public override Task<IActionResult> RunAsync(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "feature-flags")]
        HttpRequest req, FunctionContext functionContext)
    {
        return base.RunAsync(req, functionContext);
    }

    protected override async Task<ApiResult<GetFeatureFlagsResponse>> HandleRequest(EmptyRequest request,
        ILogger logger, FunctionContext functionContext)
    {
        var response = new GetFeatureFlagsResponse([]);

        var flags = typeof(Flags)
            .GetFields(BindingFlags.Public | BindingFlags.Static)
            .Where(f => f.FieldType == typeof(string))
            .Select(f => f.GetValue(null)?.ToString())
            .ToList();

        foreach (var flag in flags)
        {
            var enabled = await functionFeatureToggleHelper.IsFeatureEnabled(flag, functionContext, Principal, new NoSiteRequestInspector());
            response.FeatureFlags.Add(new KeyValuePair<string, bool>(flag, enabled));
        }

        return ApiResult<GetFeatureFlagsResponse>.Success(response);
    }

    protected override Task<IEnumerable<ErrorMessageResponseItem>> ValidateRequest(EmptyRequest request)
    {
        return Task.FromResult(Enumerable.Empty<ErrorMessageResponseItem>());
    }
}
