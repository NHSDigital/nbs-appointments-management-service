using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Nhs.Appointments.Api.Models;
using Nhs.Appointments.Core;
using Nhs.Appointments.Api.Auth;

namespace Nhs.Appointments.Api.Functions;

public class QueryBookingsFunction : BaseApiFunction<QueryBookingsRequest, IEnumerable<Booking>>
{
    private readonly IBookingsService _bookingsService;    

    public QueryBookingsFunction(
        IBookingsService bookingsService,
        IValidator<QueryBookingsRequest> validator,
        IUserContextProvider userContextProvider,
        ILogger<QueryBookingsFunction> logger) : base(validator, userContextProvider, logger)
    {
        _bookingsService = bookingsService;
    }
    
    [OpenApiOperation(operationId: "QueryBooking", tags: new [] {"Booking"}, Summary = "Query a booking")]
    [OpenApiRequestBody("text/json", typeof(QueryBookingsRequest))]
    [OpenApiSecurity("Api Key", SecuritySchemeType.ApiKey, Name = "Authorization", In = OpenApiSecurityLocationType.Header)]
    [OpenApiResponseWithBody(statusCode:HttpStatusCode.OK, contentType: "text/json", typeof(IEnumerable<Booking>), Description = "The bookings matching the query")]
    [RequiresPermission("booking:query")]
    [Function("QueryBookingsFunction")]
    public override Task<IActionResult> RunAsync(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "get-bookings")] HttpRequest req)
    {
        return base.RunAsync(req);
    }
    
    protected override async Task<ApiResult<IEnumerable<Booking>>> HandleRequest(QueryBookingsRequest request, ILogger logger)
    {
        var booking = await _bookingsService.GetBookings(request.site, request.from, request.to);
        return Success(booking);
    }
}