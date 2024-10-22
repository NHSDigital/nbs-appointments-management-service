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
using Microsoft.OpenApi.Models;
using Nhs.Appointments.Api.Auth;

namespace Nhs.Appointments.Api.Functions;

public class QueryBookingByReferenceFunction(IBookingsService bookingsService, IValidator<QueryBookingByReferenceRequest> validator, IUserContextProvider userContextProvider, ILogger<QueryBookingByReferenceFunction> logger)
    : BaseApiFunction<QueryBookingByReferenceRequest, Booking>(validator, userContextProvider, logger)
{

    [OpenApiOperation(operationId: "QueryBookingByReference", tags: ["Booking"], Summary = "Get a booking by booking reference")]
    [OpenApiParameter("bookingReference", Required = true, In = ParameterLocation.Path, Description = "The booking reference of the patients' booking to retrieve")]
    [OpenApiResponseWithBody(statusCode:HttpStatusCode.OK, contentType: "text/json", typeof(Booking), Description = "The booking for patient with the provided reference")]
    [OpenApiResponseWithBody(statusCode:HttpStatusCode.NotFound, "text/json", typeof(ApiResult<object>), Description = "Booking not found")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.BadRequest, "text/plain", typeof(ErrorMessageResponseItem), Description = "The body of the request is invalid")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, "text/plain", typeof(ErrorMessageResponseItem), Description = "Unauthorized request to a protected API")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.Forbidden, "text/plain", typeof(ErrorMessageResponseItem), Description = "Request failed due to insufficient permissions")]
    [RequiresPermission("booking:query", typeof(NoSiteRequestInspector))]
    [Function("QueryBookingByBookingReference")]
    public Task<IActionResult> RunAsync(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "booking/{bookingReference}")] HttpRequest req)
    {
        return base.RunAsync(req);
    }

    protected override async Task<ApiResult<Booking>> HandleRequest(QueryBookingByReferenceRequest request, ILogger logger)
    {
        var booking = await bookingsService.GetBookingByReference(request.bookingReference);

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