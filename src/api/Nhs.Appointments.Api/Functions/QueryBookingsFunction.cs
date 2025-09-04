using System;
using System.Collections.Generic;
using System.Linq;
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
using Nhs.Appointments.Core.Features;
using Nhs.Appointments.Core.Inspectors;

namespace Nhs.Appointments.Api.Functions;

public class QueryBookingsFunction(
    IBookingQueryService bookingQueryService,
    IValidator<QueryBookingsRequest> validator,
    IUserContextProvider userContextProvider,
    ILogger<QueryBookingsFunction> logger,
    IMetricsRecorder metricsRecorder,
    IFeatureToggleHelper featureToggleHelper)
    : BaseApiFunction<QueryBookingsRequest, IEnumerable<Booking>>(validator, userContextProvider, logger,
        metricsRecorder)
{
    [OpenApiOperation(operationId: "QueryBooking", tags: ["Booking"],
        Summary = "Query bookings for a site within a date range")]
    [OpenApiRequestBody("application/json", typeof(QueryBookingsRequest), Required = true)]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, "application/json", typeof(IEnumerable<Booking>),
        Description = "List of bookings returned from the query")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.BadRequest, "application/json",
        typeof(ErrorMessageResponseItem), Description = "The body of the request is invalid")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, "application/json",
        typeof(ErrorMessageResponseItem), Description = "Unauthorized request to a protected API")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.Forbidden, "application/json", typeof(ErrorMessageResponseItem),
        Description = "Request failed due to insufficient permissions")]
    [RequiresPermission(Permissions.QueryBooking, typeof(SiteFromBodyInspector))]
    [Function("QueryBookingsFunction")]
    public override Task<IActionResult> RunAsync(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "booking/query")]
        HttpRequest req)
    {
        return base.RunAsync(req);
    }

    protected override async Task<ApiResult<IEnumerable<Booking>>> HandleRequest(QueryBookingsRequest request,
        ILogger logger)
    {
        if (await featureToggleHelper.IsFeatureEnabled(Flags.CancelDay))
        {
            var filter = new BookingQueryFilter(request.from,
                request.to,
                request.site,
                request.statuses?.Select(Enum.Parse<AppointmentStatus>).ToArray(),
                request.cancellationReason != null ? Enum.Parse<CancellationReason>(request.cancellationReason) : null,
                request.cancellationNotificationStatuses?
                    .Select(Enum.Parse<CancellationNotificationStatus>).ToArray());

            var booking = await bookingQueryService.GetBookings(filter);
            return Success(booking);
        }
        else
        {
            var booking = await bookingQueryService.GetBookings(request.from, request.to, request.site);
            return Success(booking);
        }
    }
}
