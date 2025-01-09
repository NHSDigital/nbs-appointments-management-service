using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Nhs.Appointments.Api.Auth;
using Nhs.Appointments.Api.Models;
using Nhs.Appointments.Core;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace Nhs.Appointments.Api.Functions;
public class CancelSessionFunction(IAvailabilityService availabilityService, IBookingsService bookingService, IValidator<CancelSessionRequest> validator, IUserContextProvider userContextProvider, ILogger<CancelSessionFunction> logger, IMetricsRecorder metricsRecorder)
        : BaseApiFunction<CancelSessionRequest, EmptyResponse>(validator, userContextProvider, logger, metricsRecorder)
{
    [OpenApiOperation(operationId: "CancelSession", tags: ["Availability"], Summary = "Cancel a session")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, "application/json", typeof(CancelBookingResponse), Description = "Sessions successfully cancelled")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.BadRequest, "application/json", typeof(IEnumerable<ErrorMessageResponseItem>), Description = "The body of the request is invalid")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.NotFound, "application/json", typeof(string), Description = "Could not find session to cancel")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, "application/json", typeof(ErrorMessageResponseItem), Description = "Unauthorized request to a protected API")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.Forbidden, "application/json", typeof(ErrorMessageResponseItem), Description = "Request failed due to insufficient permissions")]
    [RequiresPermission("availability:setup", typeof(SiteFromBodyInspector))]
    [Function("CancelSessionFunction")]
    public override Task<IActionResult> RunAsync(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "session/cancel")] HttpRequest req)
    {
        return base.RunAsync(req);
    }

    protected override async Task<ApiResult<EmptyResponse>> HandleRequest(CancelSessionRequest request, ILogger logger)
    {
        var session = await availabilityService.GetSession(
            request.Site,
            DateOnly.FromDateTime(request.From.Date),
            request.From.ToShortTimeString(),
            request.Until.ToShortTimeString(),
            request.Services,
            request.SlotLength,
            request.Capacity);

        if (session is null)
        {
            return Failed(HttpStatusCode.NotFound, "The specified session was not found.");
        }

        await bookingService.OrphanAppointments(request.Site, session.From, session.Until);

        return Success(new EmptyResponse());
    }
}
