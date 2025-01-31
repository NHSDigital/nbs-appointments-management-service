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

public class GetAccessibilityDifinitionsFunction(
    IAccessibilityDefinitionsService AccessibilityDefinitionsService, 
    IValidator<EmptyRequest> validator, 
    IUserContextProvider userContextProvider, 
    ILogger<GetAccessibilityDifinitionsFunction> logger, 
    IMetricsRecorder metricsRecorder) 
: BaseApiFunction<EmptyRequest, IEnumerable<AccessibilityDefinition>>(validator, userContextProvider, logger, metricsRecorder)
{
    [OpenApiOperation(operationId: "GetAccessibilityDefinitions", tags: ["AccessibilityDefinitions"], Summary = "Get system attribute definitions")]
    [OpenApiResponseWithBody(statusCode:HttpStatusCode.OK, "application/json", typeof(IEnumerable<AccessibilityDefinition>), Description = "List of attribute definitions used by the system")]
    [OpenApiResponseWithBody(statusCode:HttpStatusCode.Unauthorized, "application/json", typeof(ErrorMessageResponseItem), Description = "Unauthorized request to a protected API")]
    [Function("GetAccessibilityDefinitionsFunction")]
    public override Task<IActionResult> RunAsync(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "AccessibilityDefinitions")] HttpRequest req)
    {
        return base.RunAsync(req);
    }
    
    protected override async Task<ApiResult<IEnumerable<AccessibilityDefinition>>> HandleRequest(EmptyRequest request, ILogger logger)
    {        
        var AccessibilityDefinitions = await AccessibilityDefinitionsService.GetAccessibilityDefinitions();
        return ApiResult<IEnumerable<AccessibilityDefinition>>.Success(AccessibilityDefinitions);
    }

    protected override Task<IEnumerable<ErrorMessageResponseItem>> ValidateRequest(EmptyRequest request)
    {
        return Task.FromResult(Enumerable.Empty<ErrorMessageResponseItem>());
    }
}
