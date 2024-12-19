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

namespace Nhs.Appointments.Api.Functions;

public class GetWellKnownOdsCodeEntriesFunction(IWellKnowOdsCodesService wellKnowOdsCodesService, IValidator<EmptyRequest> validator, IUserContextProvider userContextProvider, ILogger<GetWellKnownOdsCodeEntriesFunction> logger, IMetricsRecorder metricsRecorder) 
: BaseApiFunction<EmptyRequest, IEnumerable<WellKnownOdsEntry>>(validator, userContextProvider, logger, metricsRecorder)
{
    [OpenApiOperation(operationId: "GetWellKnownOdsCodeEntries", tags: ["wellKnownOdsCodeEntries"], Summary = "Get information for well known ods codes.")]
    [OpenApiResponseWithBody(statusCode:HttpStatusCode.OK, "application/json", typeof(IEnumerable<WellKnownOdsEntry>), Description = "List of well known ods entries used by the system")]
    [OpenApiResponseWithBody(statusCode:HttpStatusCode.Unauthorized, "application/json", typeof(ErrorMessageResponseItem), Description = "Unauthorized request to a protected API")]
    [Function("GetWellKnownOdsCodeEntriesFunction")]
    public override Task<IActionResult> RunAsync(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "wellKnownOdsCodeEntries")] HttpRequest req)
    {
        return base.RunAsync(req);
    }

    protected override async Task<ApiResult<IEnumerable<WellKnownOdsEntry>>> HandleRequest(EmptyRequest request, ILogger logger)
    {        
        var wellKnownOdsCodeEntries = await wellKnowOdsCodesService.GetWellKnownOdsCodeEntries();
        return ApiResult<IEnumerable<WellKnownOdsEntry>>.Success(wellKnownOdsCodeEntries);
    }

    protected override Task<IEnumerable<ErrorMessageResponseItem>> ValidateRequest(EmptyRequest request)
    {
        return Task.FromResult(Enumerable.Empty<ErrorMessageResponseItem>());
    }
}
