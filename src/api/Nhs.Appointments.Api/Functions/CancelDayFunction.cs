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
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace Nhs.Appointments.Api.Functions;

public class CancelDayFunction(
    IAvailabilityWriteService availabilityWriteService,
    IValidator<CancelDayRequest> validator,
    IUserContextProvider userContextProvider,
    ILogger<CancelDayFunction> logger,
    IMetricsRecorder metricsRecorder) : BaseApiFunction<CancelDayRequest, CancelDayResponse>(validator, userContextProvider, logger, metricsRecorder)
{
    [OpenApiOperation(operationId: "CancelDay", tags: ["Availability", "Booking"], Summary = "Cancel all sessions and bookings on a given day")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, "application/json", typeof(CancelDayResponse),
        Description = "All Sessions and Bookings successfully cancelled for specified day")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.BadRequest, "application/json",
        typeof(IEnumerable<ErrorMessageResponseItem>), Description = "The body of the request is invalid")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, "application/json",
        typeof(ErrorMessageResponseItem), Description = "Unauthorized request to a protected API")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.Forbidden, "application/json", typeof(ErrorMessageResponseItem),
        Description = "Request failed due to insufficient permissions")]
    [RequiresPermission(Permissions.SetupAvailability, typeof(SiteFromBodyInspector))]
    [Function("CancelDayFunction")]
    public override Task<IActionResult> RunAsync([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "day/cancel")] HttpRequest req)
    {
        return base.RunAsync(req);
    }

    protected override async Task<ApiResult<CancelDayResponse>> HandleRequest(CancelDayRequest request, ILogger logger)
    {
        var (cancelledBookingCount, bookingsWithouContactDetails) = await availabilityWriteService.CancelDayAsync(request.Site, request.Date);

        return Success(new CancelDayResponse(cancelledBookingCount, bookingsWithouContactDetails));
    }
}
