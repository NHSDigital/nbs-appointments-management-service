using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Nhs.Appointments.Core;
using FluentValidation;
using Nhs.Appointments.Api.Auth;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.OpenApi.Models;
using Nhs.Appointments.Api.Models;
using System.Collections.Generic;
using System.Net;
using Microsoft.Azure.Functions.Worker;

namespace Nhs.Appointments.Api.Functions;

public class SetTemplateFunction : BaseApiFunction<WeekTemplate, string>
{
    private readonly ITemplateService _templateService;
    private readonly IUserSiteAssignmentService _userSiteAssignmentService;

    public SetTemplateFunction(
        ITemplateService templateService,
        IUserSiteAssignmentService userSiteAssignmentService,
        IValidator<WeekTemplate> validator, 
        IUserContextProvider userContextProvider,
        ILogger<SetTemplateFunction> logger) : base(validator, userContextProvider, logger)
    {
        _templateService = templateService;
        _userSiteAssignmentService = userSiteAssignmentService;
    }

    [OpenApiOperation(operationId: "SetTemplate", tags: new[] { "Site Configuration" }, Summary = "Save a week template definition")]
    [OpenApiRequestBody("text/json", typeof(WeekTemplate))]
    [OpenApiSecurity("Api Key", SecuritySchemeType.ApiKey, Name = "Authorization", In = OpenApiSecurityLocationType.Header)]
    [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.OK, Description = "Week template successfully saved")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.BadRequest, contentType: "text/json", typeof(IEnumerable<ErrorMessageResponseItem>), Description = "The body of the request is invalid")]
    [Function("SetTemplateFunction")]
    public override Task<IActionResult> RunAsync(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "template")] HttpRequest req)
    {
        return base.RunAsync(req);
    }

    protected override async Task<ApiResult<string>> HandleRequest(WeekTemplate request, ILogger logger)
    {
        if(string.IsNullOrEmpty(request.Site))
        {
            var userEmail = Principal.Claims.GetUserEmail();
            request.Site = await _userSiteAssignmentService.GetSiteIdForUserByEmailAsync(userEmail);                
        }

        var templateId = await _templateService.SaveTemplate(request);
        return Success(templateId);
    }
}
