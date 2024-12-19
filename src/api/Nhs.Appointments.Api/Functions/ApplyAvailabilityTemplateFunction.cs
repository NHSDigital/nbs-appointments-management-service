using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Extensions.Logging;
using Nhs.Appointments.Api.Auth;
using Nhs.Appointments.Api.Availability;
using Nhs.Appointments.Api.Models;
using Nhs.Appointments.Core;

namespace Nhs.Appointments.Api.Functions;

public class ApplyAvailabilityTemplateFunction(IAvailabilityService availabilityService, IValidator<ApplyAvailabilityTemplateRequest> validator, IUserContextProvider userContextProvider, ILogger<ApplyAvailabilityTemplateFunction> logger, IMetricsRecorder metricsRecorder)
    : BaseApiFunction<ApplyAvailabilityTemplateRequest, EmptyResponse>(validator, userContextProvider, logger, metricsRecorder)
{
    [OpenApiOperation(operationId: "ApplyAvailabilityTemplate", tags: ["Availability"], Summary = "Set appointment availability for a date range")]
    [OpenApiRequestBody("application/json", typeof(ApplyAvailabilityTemplateRequest), Required = true)]
    [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.OK, Description = "Site availability successfully set or updated")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.BadRequest, "application/json", typeof(IEnumerable<ErrorMessageResponseItem>), Description = "The body of the request is invalid")]
    [OpenApiResponseWithBody(statusCode:HttpStatusCode.Unauthorized, "application/json", typeof(ErrorMessageResponseItem), Description = "Unauthorized request to a protected API")]
    [OpenApiResponseWithBody(statusCode:HttpStatusCode.Forbidden, "application/json", typeof(ErrorMessageResponseItem), Description = "Request failed due to insufficient permissions")]
    [RequiresPermission("availability:setup", typeof(SiteFromBodyInspector))]
    [Function("ApplyAvailabilityTemplateFunction")]
    public override Task<IActionResult> RunAsync(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "availability/apply-template")] HttpRequest req)
    {
        return base.RunAsync(req);
    }

    protected override async Task<ApiResult<EmptyResponse>> HandleRequest(ApplyAvailabilityTemplateRequest request, ILogger logger)
    {
        var user = userContextProvider.UserPrincipal.Claims.GetUserEmail();

        await availabilityService.ApplyAvailabilityTemplateAsync(request.Site, request.From, request.Until, request.Template, request.Mode, user);
        return Success(new EmptyResponse());
    }
}
