using Azure;
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
using Nhs.Appointments.Core.Inspectors;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace Nhs.Appointments.Api.Functions;

public class GetCancelledBySiteFunction(
    IBookingQueryService bookingQueryService,
    IValidator<GetCancelledBySiteRequest> validator,
    IUserContextProvider userContextProvider,
    ILogger<GetCancelledBySiteFunction> logger,
    IMetricsRecorder metricsRecorder)
    : BaseApiFunction<GetCancelledBySiteRequest, IEnumerable<Booking>>(validator, userContextProvider,
        logger, metricsRecorder)
{
    [OpenApiOperation(operationId: "GetCancelledBySite", tags: ["Availability"],
    Summary = "Get daily site cancelled bookings for a date and site")]
    [OpenApiParameter("site", In = ParameterLocation.Query, Required = true, Type = typeof(string),
    Description = "The ID of the site from which to query availability")]
    [OpenApiParameter("from", In = ParameterLocation.Query, Required = true, Type = typeof(double),
    Description = "The date for the selected day")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, "application/json", typeof(IEnumerable<Booking>),
    Description = "The cancelled bookings for the date and site")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, "application/json",
    typeof(ErrorMessageResponseItem), Description = "Unauthorized request to a protected API")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.Forbidden, "application/json", typeof(ErrorMessageResponseItem),
    Description = "Request failed due to insufficient permissions")]
    [RequiresPermission(Permissions.QueryAvailability, typeof(SiteFromQueryStringInspector))]
    [Function("GetCancelledBySiteFunction")]
    public override Task<IActionResult> RunAsync(
    [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "cancelled-by-site")] HttpRequest req
)
    {
        return base.RunAsync(req);
    }

    protected override async Task<ApiResult<IEnumerable<Booking>>> HandleRequest(
        GetCancelledBySiteRequest request, ILogger logger)
    {
        var filter = new BookingQueryFilter(
                from: request.FromDate.ToDateTime(TimeOnly.MinValue),
                to: request.UntilDate.ToDateTime(TimeOnly.MaxValue),
                site: request.Site,
                cancellationReason: CancellationReason.CancelledBySite
            );

        var cancelledBookings =
            await bookingQueryService.GetBookings(filter);

        return Success(cancelledBookings);
    }

    protected override Task<(IReadOnlyCollection<ErrorMessageResponseItem> errors, GetCancelledBySiteRequest request)>
        ReadRequestAsync(HttpRequest req)
    {
        var errors = new List<ErrorMessageResponseItem>();

        var site = req.Query["site"];
        var from = req.Query["from"];
        var until = req.Query["until"];

        return Task
            .FromResult<(IReadOnlyCollection<ErrorMessageResponseItem> errors, GetCancelledBySiteRequest request)>
                ((errors.AsReadOnly(), new GetCancelledBySiteRequest(site, from, until)));
    }

}
