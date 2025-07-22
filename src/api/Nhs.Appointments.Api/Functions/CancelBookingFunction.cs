using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Nhs.Appointments.Api.Auth;
using Nhs.Appointments.Api.Json;
using Nhs.Appointments.Api.Models;
using Nhs.Appointments.Audit.Functions;
using Nhs.Appointments.Core;
using Nhs.Appointments.Core.Inspectors;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace Nhs.Appointments.Api.Functions;

public class CancelBookingFunction(
    IBookingWriteService bookingWriteService,
    IValidator<CancelBookingRequest> validator,
    IUserContextProvider userContextProvider,
    ILogger<CancelBookingFunction> logger,
    IMetricsRecorder metricsRecorder)
    : BaseApiFunction<CancelBookingRequest, CancelBookingResponse>(validator, userContextProvider, logger,
        metricsRecorder)
{
    [OpenApiOperation(operationId: "CancelBooking", tags: ["Booking"], Summary = "Cancel a booking")]
    [OpenApiParameter("bookingReference", Required = true, In = ParameterLocation.Path,
        Description = "The booking reference of the booking to cancel")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, "application/json", typeof(CancelBookingResponse),
        Description = "Booking successfully cancelled")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.BadRequest, "application/json",
        typeof(IEnumerable<ErrorMessageResponseItem>), Description = "The body of the request is invalid")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.NotFound, "application/json", typeof(string),
        Description = "Requested site not configured for appointments")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, "application/json",
        typeof(ErrorMessageResponseItem), Description = "Unauthorized request to a protected API")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.Forbidden, "application/json", typeof(ErrorMessageResponseItem),
        Description = "Request failed due to insufficient permissions")]
    [RequiresPermission(Permissions.CancelBooking, typeof(SiteFromQueryStringInspector))]
    [RequiresAudit(typeof(SiteFromQueryStringInspector))]
    [Function("CancelBookingFunction")]
    public override Task<IActionResult> RunAsync(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "booking/{bookingReference}/cancel")]
        HttpRequest req)
    {
        return base.RunAsync(req);
    }

    protected override async Task<ApiResult<CancelBookingResponse>> HandleRequest(CancelBookingRequest request,
        ILogger logger)
    {
        var result = await bookingWriteService.CancelBooking(request.bookingReference, request.site, (CancellationReason)request.cancellationReason);

        switch (result)
        {
            case BookingCancellationResult.Success:
                var response = new CancelBookingResponse(request.bookingReference, "cancelled");
                return Success(response);
            case BookingCancellationResult.NotFound:
                return Failed(HttpStatusCode.NotFound, "booking not found");
            default:
                throw new Exception($"Unexpected cancellation result status: {result}");
        }
    }

    protected override async Task<(IReadOnlyCollection<ErrorMessageResponseItem> errors, CancelBookingRequest request)>
        ReadRequestAsync(HttpRequest req)
    {
        var bookingReference = req.HttpContext.GetRouteValue("bookingReference")?.ToString();
        var site = req.Query.ContainsKey("site") ? req.Query["site"].ToString() : string.Empty;
        var cancellationReason = CancellationReason.CancelledByCitizen;

        if (req.Body != null)
        {
            var (errors, payload) = await JsonRequestReader.ReadRequestAsync<CancelBookingRequest>(req.Body, true);

            if (payload?.cancellationReason != null) 
            { 
                cancellationReason = (CancellationReason)payload.cancellationReason;
            }
        }

        var requestModel = new CancelBookingRequest(bookingReference, site, cancellationReason);

        return await Task.FromResult((ErrorMessageResponseItem.None, requestModel));
    }
}
