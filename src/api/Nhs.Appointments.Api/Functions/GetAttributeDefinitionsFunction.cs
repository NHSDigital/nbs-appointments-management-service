using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Nhs.Appointments.Api.Auth;
using Nhs.Appointments.Api.Models;
using Nhs.Appointments.Core;

namespace Nhs.Appointments.Api.Functions;

public class GetAttributeSetsFunction(IAttributeDefinitionsService attributeDefinitionsService, IValidator<EmptyRequest> validator, IUserContextProvider userContextProvider, ILogger<GetAttributeSetsFunction> logger ) 
: BaseApiFunction<EmptyRequest, IEnumerable<AttributeDefinition>>(validator, userContextProvider, logger)
{
    [OpenApiOperation(operationId: "GetAttributeDefinitions", tags: new [] {"AttributeDefinitions"}, Summary = "Get system attribute sets")]
    [OpenApiSecurity("Api Key", SecuritySchemeType.ApiKey, Name = "Authorization", In = OpenApiSecurityLocationType.Header)]
    [OpenApiResponseWithBody(statusCode:HttpStatusCode.OK, "text/plain", typeof(IEnumerable<AttributeDefinition>), Description = "List of attribute definitions used by the system")]
    [OpenApiResponseWithBody(statusCode:HttpStatusCode.Forbidden, "text/plain", typeof(ErrorMessageResponseItem), Description = "Request failed due to insufficient permissions")]
    [OpenApiResponseWithBody(statusCode:HttpStatusCode.Unauthorized, "text/plain", typeof(ErrorMessageResponseItem), Description = "Unauthorized request to a protected API")]
    [RequiresPermission("site:get-config", typeof(NoSiteRequestInspector))]
    [Function("GetAttributeDefinitionsFunction")]
    public override Task<IActionResult> RunAsync(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "attributeDefinitions")] HttpRequest req)
    {
        return base.RunAsync(req);
    }
    
    protected override async Task<ApiResult<IEnumerable<AttributeDefinition>>> HandleRequest(EmptyRequest request, ILogger logger)
    {        
        var attributeDefinitions = await attributeDefinitionsService.GetAttributeDefinitions();
        return ApiResult<IEnumerable<AttributeDefinition>>.Success(attributeDefinitions);
    }

    protected override Task<IEnumerable<ErrorMessageResponseItem>> ValidateRequest(EmptyRequest request)
    {
        return Task.FromResult(Enumerable.Empty<ErrorMessageResponseItem>());
    }
}
