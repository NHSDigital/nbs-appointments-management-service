using System.Threading.Tasks;
using System.Collections.Generic;
using System.Net;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.OpenApi.Models;
using FluentValidation;
using Microsoft.Azure.Functions.Worker;
using Nhs.Appointments.Core;
using Nhs.Appointments.Api.Models;
using Nhs.Appointments.Api.Auth;

namespace Nhs.Appointments.Api.Functions;

public class SetTemplateAssignmentsFunction : BaseApiFunction<SetTemplateAssignmentRequest, EmptyResponse>
{
    private readonly ITemplateService _templateService;
    public SetTemplateAssignmentsFunction(
        ITemplateService templateService,
        IValidator<SetTemplateAssignmentRequest> validator, 
        IUserContextProvider userContextProvider,
        ILogger<SetTemplateAssignmentsFunction> logger) : base(validator, userContextProvider, logger)
    {
        _templateService = templateService;
    }

    [OpenApiOperation(operationId: "SetTemplateAssignments", tags: new[] { "Site Configuration" }, Summary = "Save template assignments for a site")]
    [OpenApiRequestBody("text/json", typeof(SetTemplateAssignmentRequest))]
    [OpenApiSecurity("Api Key", SecuritySchemeType.ApiKey, Name = "Authorization", In = OpenApiSecurityLocationType.Header)]
    [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.OK, Description = "Template assignments successfully saved")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.BadRequest, contentType: "text/json", typeof(IEnumerable<ErrorMessageResponseItem>), Description = "The body of the request is invalid")]
    [RequiresPermission("availability:set-setup", typeof(SiteFromBodyInspector))]
    [Function("SetTemplateAssignments")]
    public override Task<IActionResult> RunAsync(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "templates/assignments")] HttpRequest req)
    {
        return base.RunAsync(req);
    }

    protected override async Task<ApiResult<EmptyResponse>> HandleRequest(SetTemplateAssignmentRequest request, ILogger logger)
    {
        var assignments = request.Assignments.Select(a => new Core.TemplateAssignment
        {
            From = a.FromDate,
            Until = a.UntilDate,
            TemplateId = a.TemplateId
        });
        await _templateService.SaveAssignmentsAsync(request.Site, assignments);
        return Success(new EmptyResponse());
    }
}        
