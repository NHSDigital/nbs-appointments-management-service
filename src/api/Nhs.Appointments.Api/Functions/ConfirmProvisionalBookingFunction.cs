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
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.OpenApi.Models;
using Nhs.Appointments.Api.Auth;
using System.Linq;
using System.Collections.Generic;
using Microsoft.AspNetCore.Routing;

namespace Nhs.Appointments.Api.Functions;

public class ConfirmProvisionalBookingFunction(IBookingsService bookingService,
        IValidator<ConfirmBookingRequest> validator,
        IUserContextProvider userContextProvider,
        ILogger<MakeBookingFunction> logger,
        IMetricsRecorder metricsRecorder) : BaseApiFunction<ConfirmBookingRequest, EmptyResponse>(validator, userContextProvider, logger, metricsRecorder)
{        
    [OpenApiOperation(operationId: "ConfirmProvisionalBooking", tags: ["Booking"], Summary = "Confirm a provisional booking")]
    [OpenApiRequestBody("application/json", typeof(ConfirmBookingRequest), Required = true)]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, "application/json", typeof(EmptyResponse), Description = "Returns 200 OK if booking was confirmed")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.BadRequest, "application/json", typeof(IEnumerable<ErrorMessageResponseItem>), Description = "The body of the request is invalid")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, "application/json", typeof(ErrorMessageResponseItem), Description = "Unauthorized request to a protected API")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.Forbidden, "application/json", typeof(ErrorMessageResponseItem), Description = "Request failed due to insufficient permissions")]
    [RequiresPermission("booking:make", typeof(SiteFromPathInspector))]
    [Function("ConfirmProvisionalBookingFunction")]
    public override Task<IActionResult> RunAsync(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "sites/{site}/booking/{bookingReference}/confirm")] HttpRequest req)
    {
        return base.RunAsync(req);
    }

    protected override async Task<ApiResult<EmptyResponse>> HandleRequest(ConfirmBookingRequest bookingRequest, ILogger logger)
    {
        var success = await bookingService.ConfirmProvisionalBooking(bookingRequest.bookingReference, bookingRequest.contactDetails.Select(x => new Core.ContactItem { Type = x.Type, Value = x.Value }));

        if (success)
        {
            return Success();
        }

        return Failed(HttpStatusCode.NotFound, "The booking was not found");
    }

    protected override async Task<(bool requestRead, ConfirmBookingRequest request)> ReadRequestAsync(HttpRequest req)
    {
        var baseRequest = await base.ReadRequestAsync(req);
        var bookingReference = req.HttpContext.GetRouteValue("bookingReference")?.ToString();

        return (true, new ConfirmBookingRequest(bookingReference, baseRequest.request.contactDetails));        
    }
}