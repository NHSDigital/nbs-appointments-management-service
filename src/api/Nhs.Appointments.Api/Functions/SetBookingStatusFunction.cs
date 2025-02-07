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

public class SetBookingStatusFunction(IBookingsService bookingService, IValidator<SetBookingStatusRequest> validator, IUserContextProvider userContextProvider, ILogger<SetBookingStatusFunction> logger, IMetricsRecorder metricsRecorder)
    : BaseApiFunction<SetBookingStatusRequest, SetBookingStatusResponse>(validator, userContextProvider, logger, metricsRecorder)
{

    [OpenApiOperation(operationId: "SetBookingStatus", tags: ["Booking"], Summary = "Set the status of a booking")]
    [OpenApiRequestBody("application/json", typeof(SetBookingStatusRequest), Required = true)]
    [OpenApiResponseWithBody(statusCode:HttpStatusCode.OK, "application/json", typeof(SetBookingStatusResponse), Description = "Booking status successfully set")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.BadRequest, "application/json", typeof(ErrorMessageResponseItem), Description = "The body of the request is invalid")]
    [OpenApiResponseWithBody(statusCode:HttpStatusCode.NotFound, "application/json", typeof(ApiResult<object>), Description = "Booking not found")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, "application/json", typeof(ErrorMessageResponseItem), Description = "Unauthorized request to a protected API")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.Forbidden, "application/json", typeof(ErrorMessageResponseItem), Description = "Request failed due to insufficient permissions")]
    [RequiresPermission(Permissions.SetBookingStatus, typeof(SiteFromBodyInspector))]
    [Function("SetBookingStatusFunction")]
    public override Task<IActionResult> RunAsync(
       [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "booking/set-status")] HttpRequest req)
    {
        return base.RunAsync(req);
    }

    protected override async Task<ApiResult<SetBookingStatusResponse>> HandleRequest(SetBookingStatusRequest request, ILogger logger)
    {
        var result = await bookingService.SetBookingStatus(request.bookingReference, request.status,
            DeriveAvailabilityStatusFromAppointmentStatus(request.status));
        return result ? Success(new SetBookingStatusResponse(request.bookingReference, request.status))
            : Failed(
                HttpStatusCode.NotFound, $"Booking {request.bookingReference} not found");
    }

    private static AvailabilityStatus
        DeriveAvailabilityStatusFromAppointmentStatus(AppointmentStatus appointmentStatus) =>
        appointmentStatus is AppointmentStatus.Cancelled or AppointmentStatus.Unknown
            ? AvailabilityStatus.Unknown
            : AvailabilityStatus.Supported;
}
