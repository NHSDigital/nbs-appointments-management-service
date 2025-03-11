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
using Nhs.Appointments.Api.Models;
using Nhs.Appointments.Core;
using Nhs.Appointments.Core.Inspectors;

namespace Nhs.Appointments.Api.Functions;

public class CancelSessionFunction(
    IAvailabilityService availabilityService,
    IBookingsService bookingService,
    IValidator<CancelSessionRequest> validator,
    IUserContextProvider userContextProvider,
    ILogger<CancelSessionFunction> logger,
    IMetricsRecorder metricsRecorder)
    : BaseApiFunction<CancelSessionRequest, EmptyResponse>(validator, userContextProvider, logger, metricsRecorder)
{
    [OpenApiOperation(operationId: "CancelSession", tags: ["Availability"], Summary = "Cancel a session")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, "application/json", typeof(CancelBookingResponse),
        Description = "Sessions successfully cancelled")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.BadRequest, "application/json",
        typeof(IEnumerable<ErrorMessageResponseItem>), Description = "The body of the request is invalid")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.NotFound, "application/json", typeof(string),
        Description = "Could not find session to cancel")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, "application/json",
        typeof(ErrorMessageResponseItem), Description = "Unauthorized request to a protected API")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.Forbidden, "application/json", typeof(ErrorMessageResponseItem),
        Description = "Request failed due to insufficient permissions")]
    [RequiresPermission(Permissions.SetupAvailability, typeof(SiteFromBodyInspector))]
    [Function("CancelSessionFunction")]
    public override Task<IActionResult> RunAsync(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "session/cancel")]
        HttpRequest req)
    {
        return base.RunAsync(req);
    }

    protected override async Task<ApiResult<EmptyResponse>> HandleRequest(CancelSessionRequest request, ILogger logger
)
    {
        await availabilityService.CancelSession(
            request.Site,
            request.Date,
            request.From,
            request.Until,
            request.Services,
            request.SlotLength,
            request.Capacity);

        if (TemporaryFeatureToggles.MultiServiceAvailabilityCalculations)
        {
            if (TemporaryFeatureToggles.MultiServiceAvailabilityCalculationsV2)
            {
                await availabilityService.RecalculateAppointmentStatusesV2(request.Site, request.Date, request.Date);
            }
            
            await availabilityService.RecalculateAppointmentStatuses(request.Site, request.Date);
        }
        else
        {
            await bookingService.RecalculateAppointmentStatuses(request.Site, request.Date);
        }

        return Success(new EmptyResponse());
    }
}
