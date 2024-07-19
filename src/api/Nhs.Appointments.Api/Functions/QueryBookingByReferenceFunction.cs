using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Nhs.Appointments.Core;
using Nhs.Appointments.Api.Models;
using FluentValidation;
using Microsoft.AspNetCore.Routing;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.OpenApi.Models;
using Nhs.Appointments.Api.Auth;

namespace Nhs.Appointments.Api.Functions;

public class QueryBookingByReferenceFunction : BaseApiFunction<QueryBookingByReferenceRequest, Booking>
{
    private readonly IBookingsService _bookingsService;

    public QueryBookingByReferenceFunction(
        IBookingsService bookingsService, 
        IValidator<QueryBookingByReferenceRequest> validator,
        IUserContextProvider userContextProvider,
        ILogger<QueryBookingByReferenceFunction> logger) : base(validator, userContextProvider, logger)
    {
        _bookingsService = bookingsService;
    }
    
    [OpenApiOperation(operationId: "QueryBookingByReference", tags: new [] {"Booking"}, Summary = "Query a booking by booking reference")]
    [OpenApiParameter("bookingReference", Required = true, In = ParameterLocation.Path, Description = "The booking reference of the patients' bookings to retrieve")]
    [OpenApiSecurity("Api Key", SecuritySchemeType.ApiKey, Name = "Authorization", In = OpenApiSecurityLocationType.Header)]
    [OpenApiResponseWithBody(statusCode:HttpStatusCode.OK, contentType: "text/json", typeof(Booking), Description = "The booking for patient with the provided reference")]
    [OpenApiResponseWithBody(statusCode:HttpStatusCode.NotFound, "text/json", typeof(ApiResult<object>), Description = "Booking not found")]
    [RequiresPermission("booking:query", typeof(NoSiteRequestInspector))]
    [Function("QueryBookingByBookingReference")]
    public Task<IActionResult> RunAsync(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "booking/{bookingReference}")] HttpRequest req)
    {
        return base.RunAsync(req);
    }

    protected override async Task<ApiResult<Booking>> HandleRequest(QueryBookingByReferenceRequest request, ILogger logger)
    {
        var booking = await _bookingsService.GetBookingByReference(request.bookingReference);

        if (booking is null)
        {
            return Failed(HttpStatusCode.NotFound, "Booking not found");
        }

        return Success(booking);
    }

    protected override Task<(bool requestRead, QueryBookingByReferenceRequest request)> ReadRequestAsync(HttpRequest req)
    {
        var bookingReference = req.HttpContext.GetRouteValue("bookingReference")?.ToString();
        return Task.FromResult((true, new QueryBookingByReferenceRequest(bookingReference)));
    }
}