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
using Nhs.Appointments.Api.Features;
using Nhs.Appointments.Api.Models;
using Nhs.Appointments.Core;
using Nhs.Appointments.Core.Inspectors;

namespace Nhs.Appointments.Api.Functions;

public class GetFeatureFlagsByUserFunction(
    IValidator<EmptyRequest> validator,
    IUserContextProvider userContextProvider,
    ILogger<GetFeatureFlagsByUserFunction> logger,
    IMetricsRecorder metricsRecorder,
    IFeatureToggleHelper featureToggleHelper)
    : BaseApiFunction<EmptyRequest, GetFeatureFlagsResponse>(validator, userContextProvider, logger, metricsRecorder)
{
    private FunctionContext FunctionContext { get; set; }
    
    [OpenApiOperation(operationId: "GetFeatureFlagsByUser", tags: ["FeatureFlag"],
        Summary =
            "Get the enabled state for all the defined feature flags, using the calling users id for any feature filters")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, "application/json", typeof(GetFeatureFlagsResponse),
        Description = "The enabled state for all the defined feature flags")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.BadRequest, "application/json",
        typeof(IEnumerable<ErrorMessageResponseItem>), Description = "The body of the request is invalid")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, "application/json",
        typeof(ErrorMessageResponseItem), Description = "Unauthorized request to a protected API")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.Forbidden, "application/json", typeof(ErrorMessageResponseItem),
        Description = "Request failed due to insufficient permissions")]
    [Function("GetFeatureFlagsByUserFunction")]
    public Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "feature-flags/by-user")]
        HttpRequest req, FunctionContext functionContext)
    {
        FunctionContext = functionContext;
        return base.RunAsync(req);
    }

    protected override async Task<ApiResult<GetFeatureFlagsResponse>> HandleRequest(EmptyRequest request,
        ILogger logger)
    {
        var response = new GetFeatureFlagsResponse([]);

        var flags = typeof(Flags)
            .GetFields(BindingFlags.Public | BindingFlags.Static)
            .Where(f => f.FieldType == typeof(string))
            .Select(f => f.GetValue(null)?.ToString())
            .ToList();

        foreach (var flag in flags)
        {
            var enabled = await featureToggleHelper.IsFeatureEnabledForFunction(flag, FunctionContext, Principal,
                new NoSiteRequestInspector());
            response.FeatureFlags.Add(new KeyValuePair<string, bool>(flag, enabled));
        }

        return ApiResult<GetFeatureFlagsResponse>.Success(response);
    }

    protected override Task<IEnumerable<ErrorMessageResponseItem>> ValidateRequest(EmptyRequest request)
    {
        return Task.FromResult(Enumerable.Empty<ErrorMessageResponseItem>());
    }
}
