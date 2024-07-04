using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Nhs.Appointments.Api.Auth;
using Nhs.Appointments.Core;
using FluentValidation;
using System.Linq;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.OpenApi.Models;
using Nhs.Appointments.Api.Models;
using System.Collections.Generic;
using System.Net;
using Microsoft.Azure.Functions.Worker;

namespace Nhs.Appointments.Api.Functions;

public class GetTemplateAssignmentsFunction(ITemplateService templateService, IValidator<SiteBasedResourceRequest> validator, IUserContextProvider userContextProvider, ILogger<GetTemplateAssignmentsFunction> logger)
    : SiteBasedResourceFunction<GetTemplateAssignmentsResponse>(validator, userContextProvider, logger)
{

    [OpenApiOperation(operationId: "GetTemplateAssignments", tags: new[] { "Site Configuration" }, Summary = "Get data about the week template assignments")]
    [OpenApiParameter("site", Required = true, In = ParameterLocation.Query, Description = "The site for which to retrieve week template assignments")]
    [OpenApiSecurity("Api Key", SecuritySchemeType.ApiKey, Name = "Authorization", In = OpenApiSecurityLocationType.Header)]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "text/json", typeof(GetTemplateAssignmentsResponse), Description = "The meta data for the specified site")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.BadRequest, contentType: "text/json", typeof(IEnumerable<ErrorMessageResponseItem>), Description = "The request did not contain a valid site in the query string")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.NotFound, "text/json", typeof(ApiResult<object>), Description = "No data was found for the specified site")]    
    [RequiresPermission("availability:get-setup")]
    [Function("GetTemplateAssignmentsFunction")]
    public override Task<IActionResult> RunAsync(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "templates/assignments")] HttpRequest req)
    {
        return base.RunAsync(req);
    }

    protected override async Task<ApiResult<GetTemplateAssignmentsResponse>> HandleRequest(SiteBasedResourceRequest request, ILogger logger)
    {
        var assignments = await templateService.GetAssignmentsAsync(request.Site);
        var mappedAssignments = assignments.Select(a => new Models.TemplateAssignment(
            a.From.ToString("yyyy-MM-dd"),
            a.Until.ToString("yyyy-MM-dd"),
            a.TemplateId
        ));
        return Success(new GetTemplateAssignmentsResponse(mappedAssignments.ToArray()));
    }    
}
