using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using FluentValidation;
using Nhs.Appointments.Api.Auth;
using Nhs.Appointments.Core;
using System.Linq;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.OpenApi.Models;
using Nhs.Appointments.Api.Models;
using System.Collections.Generic;
using System.Net;
using Microsoft.Azure.Functions.Worker;

namespace Nhs.Appointments.Api.Functions;

public class GetTemplatesFunction(ITemplateService templateService, IValidator<SiteBasedResourceRequest> validator, IUserContextProvider userContextProvider, ILogger<GetTemplatesFunction> logger) : SiteBasedResourceFunction<GetTemplateResponse>(validator, userContextProvider, logger)
{
    [OpenApiOperation(operationId: "GetTemplates", tags: new[] { "Site Configuration" }, Summary = "Get week templates")]
    [OpenApiParameter("site", Required = true, In = ParameterLocation.Query, Description = "The site for which to retrieve week template assignments")]
    [OpenApiSecurity("Api Key", SecuritySchemeType.ApiKey, Name = "Authorization", In = OpenApiSecurityLocationType.Header)]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "text/json", typeof(GetTemplateResponse), Description = "The week templates for the specified site")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.BadRequest, contentType: "text/json", typeof(IEnumerable<ErrorMessageResponseItem>), Description = "The request did not contain a valid site in the query string")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.NotFound, "text/json", typeof(ApiResult<object>), Description = "No data was found for the specified site")]
    [RequiresPermission("availability:get-setup", typeof(SiteFromQueryStringInspector))]
    [Function("GetTemplatesFunction")]
    public override Task<IActionResult> RunAsync(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "templates")] HttpRequest req)
    {
        return base.RunAsync(req);
    }

    protected override async Task<ApiResult<GetTemplateResponse>> HandleRequest(SiteBasedResourceRequest request, ILogger logger)
    {        
        var templates = await templateService.GetTemplates(request.Site);
        return Success(new GetTemplateResponse
        {
            Templates = templates.ToArray()
        });        
    }    
}
