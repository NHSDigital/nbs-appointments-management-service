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

namespace Nhs.Appointments.Api.Functions;

public class SetAvailabilityFunction(IAvailabilityService availabilityService, IValidator<SetAvailabilityRequest> validator, IUserContextProvider userContextProvider, ILogger<SetAvailabilityFunction> logger)
    : BaseApiFunction<SetAvailabilityRequest, EmptyResponse>(validator, userContextProvider, logger)
{
    [OpenApiOperation(operationId: "SetAvailability", tags: ["Appointment Availability"], Summary = "Set appointment availability")]
    [OpenApiRequestBody("text/json", typeof(SetAvailabilityRequest), Required = true)]
    [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.OK, Description = "Site availability successfully set or updated")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.BadRequest, "text/plain", typeof(ErrorMessageResponseItem), Description = "The body of the request is invalid")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, "text/plain", typeof(ErrorMessageResponseItem), Description = "Unauthorized request to a protected API")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.Forbidden, "text/plain", typeof(ErrorMessageResponseItem), Description = "Request failed due to insufficient permissions")]
    [RequiresPermission("availability:set-setup", typeof(SiteFromBodyInspector))]
    [Function("SetAvailabilityFunction")]
    public override Task<IActionResult> RunAsync(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "availability")] HttpRequest req)
    {
        return base.RunAsync(req);
    }

    protected override async Task<ApiResult<EmptyResponse>> HandleRequest(SetAvailabilityRequest request, ILogger logger)
    {
        await availabilityService.SetAvailabilityAsync(request.AvailabilityDate, request.Site, request.Sessions);
        return Success();
    }
}

