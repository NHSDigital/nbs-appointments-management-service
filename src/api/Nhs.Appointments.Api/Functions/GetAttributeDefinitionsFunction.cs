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

public class GetAttributeSetsFunction(IAttributeDefinitionsService attributeDefinitionsService, IValidator<EmptyRequest> validator, IUserContextProvider userContextProvider, ILogger<GetAttributeSetsFunction> logger ) 
: BaseApiFunction<EmptyRequest, IEnumerable<AttributeDefinition>>(validator, userContextProvider, logger)
{
    [OpenApiOperation(operationId: "GetAttributeDefinitions", tags: ["AttributeDefinitions"], Summary = "Get system attribute definitions")]
    [OpenApiResponseWithBody(statusCode:HttpStatusCode.OK, "application/json", typeof(IEnumerable<AttributeDefinition>), Description = "List of attribute definitions used by the system")]
    [OpenApiResponseWithBody(statusCode:HttpStatusCode.Unauthorized, "application/json", typeof(ErrorMessageResponseItem), Description = "Unauthorized request to a protected API")]
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
