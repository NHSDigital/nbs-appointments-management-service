using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Extensions.Logging;
using Nhs.Appointments.Api.Models;
using Nhs.Appointments.Core.Availability;
using Nhs.Appointments.Core.Features;
using Nhs.Appointments.Core.Users;
using System;
using System.Net;
using System.Threading.Tasks;

namespace Nhs.Appointments.Api.Functions.HttpFunctions;

public class CancelDateRangeFunction(
    IAvailabilityWriteService availabilityWriteService,
    IValidator<CancelDateRangeRequest> validator,
    IUserContextProvider userContextProvider,
    ILogger<CancelDateRangeFunction> logger,
    IMetricsRecorder metricsRecorder,
    IFeatureToggleHelper featureToggleHelper)
    : BaseApiFunction<CancelDateRangeRequest, CancelDateRangeResponse>(
        validator,
        userContextProvider,
        logger,
        metricsRecorder
    )
{
    [OpenApiOperation(operationId: "CancelDateRange", tags: ["availability"],
        Summary = "Cancel sessions (and potentially bookings) for a site in a given date range.")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, "application/json",
        typeof(EmptyResponse),
        Description = "Count of cancelled sessions and (if applicable) cancelled bookings and bookings without contact details")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, "application/json",
        typeof(ErrorMessageResponseItem), Description = "Unauthorized request to a protected API")]
    [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.NotImplemented,
        Description = "The cancel date range function is disabled or not available.")]
    [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.BadRequest, Description = "The body of the request is invalid.")]
    [Function("CancelDateRangeFunction")]
    public override async Task<IActionResult> RunAsync(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "availability/cancel-date-range")]
        HttpRequest req)
    {
        return await featureToggleHelper.IsFeatureEnabled(Flags.CancelADateRange)
            ? await base.RunAsync(req)
            : ProblemResponse(HttpStatusCode.NotImplemented, null);
    }

    protected override async Task<ApiResult<CancelDateRangeResponse>> HandleRequest(CancelDateRangeRequest request, ILogger logger)
    {
        var cancelDateRangeWithBookings = await featureToggleHelper.IsFeatureEnabled(Flags.CancelADateRangeWithBookings);
        if (!cancelDateRangeWithBookings && request.CancelBookings)
        {
            return Failed(HttpStatusCode.NotImplemented, "Cancelling bookings when cancelling a date range is not currently supported.");
        }

        var (cancelledSessionCount, cancelledBookingsCount, bookingsWithoutContactDetailsCount) = await availabilityWriteService.CancelDateRangeAsync(
            request.Site, request.From, request.To, request.CancelBookings, cancelDateRangeWithBookings);

        return Success(new CancelDateRangeResponse(cancelledSessionCount, cancelledBookingsCount, bookingsWithoutContactDetailsCount));
    }
}
