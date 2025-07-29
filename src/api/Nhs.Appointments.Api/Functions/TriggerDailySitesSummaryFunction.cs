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
using Nhs.Appointments.Api.Auth;
using Nhs.Appointments.Api.Models;
using Nhs.Appointments.Core;
using Nhs.Appointments.Core.Inspectors;
using Nhs.Appointments.Core.Reports.SiteSummary;

namespace Nhs.Appointments.Api.Functions;

public class TriggerDailySitesSummaryFunction(
    ISitesSummaryTrigger sitesSummaryTrigger,
    IValidator<EmptyRequest> validator,
    IUserContextProvider userContextProvider,
    ILogger<TriggerDailySitesSummaryFunction> logger,
    IMetricsRecorder metricsRecorder)
    : BaseApiFunction<EmptyRequest, EmptyResponse>(validator, userContextProvider, logger, metricsRecorder)
{
    [OpenApiOperation(operationId: "TriggerDailySitesSummaryFunction", tags: ["System"],
        Summary = "Utility function to manually trigger daily Sites summary")]
    [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.OK,
        Description = "Daily Sites summary triggered")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, "application/json",
        typeof(ErrorMessageResponseItem), Description = "Unauthorized request to a protected API")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.Forbidden, "application/json", typeof(ErrorMessageResponseItem),
        Description = "Request failed due to insufficient permissions")]
    [RequiresPermission(Permissions.SystemRunReminders, typeof(NoSiteRequestInspector))]
    [Function("TriggerDailySitesSummary")]
    public override Task<IActionResult> RunAsync(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "system/daily-site-summary")]
        HttpRequest req)
    {
        return base.RunAsync(req);
    }

    protected override async Task<ApiResult<EmptyResponse>> HandleRequest(EmptyRequest request, ILogger logger)
    {
        await sitesSummaryTrigger.Trigger();
        return Success(new EmptyResponse());
    }

    protected override Task<IEnumerable<ErrorMessageResponseItem>> ValidateRequest(EmptyRequest request)
    {
        return Task.FromResult(Enumerable.Empty<ErrorMessageResponseItem>());
    }
}
