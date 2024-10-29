using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Nhs.Appointments.Api.Models;
using Nhs.Appointments.Core;
using FluentValidation;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Nhs.Appointments.Api.Auth;

namespace Nhs.Appointments.Api.Functions;

public class CancelBookingFunction(IBookingsService bookingService, IValidator<CancelBookingRequest> validator, IUserContextProvider userContextProvider, ILogger<CancelBookingFunction> logger, IMetricsRecorder metricsRecorder)
    : BaseApiFunction<CancelBookingRequest, CancelBookingResponse>(validator, userContextProvider, logger, metricsRecorder)
{

    [OpenApiOperation(operationId: "CancelBooking", tags: ["Booking"], Summary = "Cancel a booking")]
    [OpenApiRequestBody("application/json", typeof(CancelBookingRequest), Required = true)]
    [OpenApiResponseWithBody(statusCode:HttpStatusCode.OK, "application/json", typeof(CancelBookingResponse), Description = "Booking successfully cancelled")]
    [OpenApiResponseWithBody(statusCode:HttpStatusCode.BadRequest, "application/json", typeof(IEnumerable<ErrorMessageResponseItem>),  Description = "The body of the request is invalid" )]
    [OpenApiResponseWithBody(statusCode:HttpStatusCode.NotFound, "application/json", typeof(string), Description = "Requested site not configured for appointments")]
    [OpenApiResponseWithBody(statusCode:HttpStatusCode.Unauthorized, "application/json", typeof(ErrorMessageResponseItem), Description = "Unauthorized request to a protected API")]
    [OpenApiResponseWithBody(statusCode:HttpStatusCode.Forbidden, "application/json", typeof(ErrorMessageResponseItem), Description = "Request failed due to insufficient permissions")]
    [RequiresPermission("booking:cancel", typeof(SiteFromBodyInspector))]
    [Function("CancelBookingFunction")]
    public override Task<IActionResult> RunAsync(
       [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "booking/cancel")] HttpRequest req)
    {
        return base.RunAsync(req);
    }

    protected override async Task<ApiResult<CancelBookingResponse>> HandleRequest(CancelBookingRequest request, ILogger logger)
    {
        await bookingService.CancelBooking(request.site, request.bookingReference);
        var response = new CancelBookingResponse(request.bookingReference, "cancelled");
        return Success(response);
    }    
}