using System.Collections.Generic;
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

public class ApplyAvailabilityTemplateFunction(IAvailabilityService availabilityService, IValidator<ApplyAvailabilityTemplateRequest> validator, IUserContextProvider userContextProvider, ILogger<ApplyAvailabilityTemplateFunction> logger) 
    : BaseApiFunction<ApplyAvailabilityTemplateRequest, EmptyResponse>(validator, userContextProvider, logger)
{
    [OpenApiOperation(operationId: "ApplyAvailabilityTemplate", tags: ["Apply availability template"], Summary = "Apply an availability template")]
    [OpenApiRequestBody("text/json", typeof(ApplyAvailabilityTemplateRequest))]
    [OpenApiSecurity("Api Key", SecuritySchemeType.ApiKey, Name = "Authorization", In = OpenApiSecurityLocationType.Header)]
    [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.OK, Description = "Availability template is applied")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.BadRequest, contentType: "text/json", typeof(IEnumerable<ErrorMessageResponseItem>), Description = "The body of the request is invalid")]
    [OpenApiResponseWithBody(statusCode:HttpStatusCode.Unauthorized, "text/plain", typeof(ErrorMessageResponseItem), Description = "Unauthorized request to a protected API")]
    [OpenApiResponseWithBody(statusCode:HttpStatusCode.Forbidden, "text/plain", typeof(ErrorMessageResponseItem), Description = "Request failed due to insufficient permissions")]
    [RequiresPermission("availability:set-setup", typeof(SiteFromBodyInspector))]
    [Function("ApplyAvailabilityTemplateFunction")]
    public override Task<IActionResult> RunAsync(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "availability/apply-template")] HttpRequest req)
    {
        return base.RunAsync(req);
    }

    protected override async Task<ApiResult<EmptyResponse>> HandleRequest(ApplyAvailabilityTemplateRequest request, ILogger logger)
    {
        await availabilityService.ApplyAvailabilityTemplateAsync(request.Site, request.FromDate, request.UntilDate, request.Template);
        return Success(new EmptyResponse());
    }
}
