using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Extensions.Logging;
using Nhs.Appointments.Api.Models;
using Nhs.Appointments.Core;
using Nhs.Appointments.Api.Auth;

namespace Nhs.Appointments.Api.Functions;

public class QueryBookingsFunction(IBookingsService bookingsService, IValidator<QueryBookingsRequest> validator, IUserContextProvider userContextProvider, ILogger<QueryBookingsFunction> logger)
    : BaseApiFunction<QueryBookingsRequest, IEnumerable<Booking>>(validator, userContextProvider, logger)
{

    [OpenApiOperation(operationId: "QueryBooking", tags: ["Booking"], Summary = "Query bookings for a site within a date range")]
    [OpenApiRequestBody("text/json", typeof(QueryBookingsRequest))]
    [OpenApiResponseWithBody(statusCode:HttpStatusCode.OK, contentType: "text/json", typeof(IEnumerable<Booking>), Description = "List of bookings returned from the query")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.BadRequest, "text/plain", typeof(ErrorMessageResponseItem), Description = "The body of the request is invalid")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, "text/plain", typeof(ErrorMessageResponseItem), Description = "Unauthorized request to a protected API")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.Forbidden, "text/plain", typeof(ErrorMessageResponseItem), Description = "Request failed due to insufficient permissions")]
    [RequiresPermission("booking:query", typeof(SiteFromBodyInspector))]
    [Function("QueryBookingsFunction")]
    public override Task<IActionResult> RunAsync(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "booking/query")] HttpRequest req)
    {
        return base.RunAsync(req);
    }
    
    protected override async Task<ApiResult<IEnumerable<Booking>>> HandleRequest(QueryBookingsRequest request, ILogger logger)
    {
        var booking = await bookingsService.GetBookings(request.site, request.from, request.to);
        return Success(booking);
    }
}