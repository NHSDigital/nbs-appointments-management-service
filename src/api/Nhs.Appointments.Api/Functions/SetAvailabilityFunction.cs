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
using System.Net;
using System.Threading.Tasks;
using Nhs.Appointments.Audit;
using Nhs.Appointments.Audit.Functions;
using Nhs.Appointments.Core.Inspectors;

namespace Nhs.Appointments.Api.Functions;

public class SetAvailabilityFunction(IAvailabilityService availabilityService, IValidator<SetAvailabilityRequest> validator, IUserContextProvider userContextProvider, ILogger<SetAvailabilityFunction> logger, IMetricsRecorder metricsRecorder)
    : BaseApiFunction<SetAvailabilityRequest, EmptyResponse>(validator, userContextProvider, logger, metricsRecorder)
{
    [OpenApiOperation(operationId: "SetAvailability", tags: ["Availability"], Summary = "Set appointment availability for a single day")]
    [OpenApiRequestBody("application/json", typeof(SetAvailabilityRequest), Required = true)]
    [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.OK, Description = "Site availability successfully set or updated")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.BadRequest, "application/json", typeof(ErrorMessageResponseItem), Description = "The body of the request is invalid")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, "application/json", typeof(ErrorMessageResponseItem), Description = "Unauthorized request to a protected API")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.Forbidden, "application/json", typeof(ErrorMessageResponseItem), Description = "Request failed due to insufficient permissions")]
    [RequiresPermission(Permissions.SetupAvailability, typeof(SiteFromBodyInspector))]
    [RequiresAudit(typeof(SiteFromBodyInspector))]
    [Function("SetAvailabilityFunction")]
    public override Task<IActionResult> RunAsync(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "availability")] HttpRequest req)
    {
        return base.RunAsync(req);
    }

    protected override async Task<ApiResult<EmptyResponse>> HandleRequest(SetAvailabilityRequest request, ILogger logger)
    {
        var user = userContextProvider.UserPrincipal.Claims.GetUserEmail();

        await availabilityService.ApplySingleDateSessionAsync(request.Date, request.Site, request.Sessions,
            request.Mode, user, request.SessionToEdit);
        return Success(new EmptyResponse());
    }
}

