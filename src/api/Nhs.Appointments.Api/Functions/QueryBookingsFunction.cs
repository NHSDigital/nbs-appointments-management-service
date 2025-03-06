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

public class QueryBookingsFunction(
    IBookingsService bookingsService,
    IValidator<QueryBookingsRequest> validator,
    IUserContextProvider userContextProvider,
    ILogger<QueryBookingsFunction> logger,
    IMetricsRecorder metricsRecorder)
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
        HttpRequest req, FunctionContext functionContext)
    {
        return base.RunAsync(req, functionContext);
    }

    protected override async Task<ApiResult<IEnumerable<Booking>>> HandleRequest(QueryBookingsRequest request,
        ILogger logger, FunctionContext functionContext)
    {
        var booking = await bookingsService.GetBookings(request.from, request.to, request.site);
        return Success(booking);
    }
}
