using System;
using System.Collections.Generic;
using System.Linq;
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

public class MakeBookingFunction(
    IBookingWriteService bookingWriteService,
    ISiteService siteService,
    IValidator<MakeBookingRequest> validator,
    IUserContextProvider userContextProvider,
    ILogger<MakeBookingFunction> logger,
    IMetricsRecorder metricsRecorder)
    : BaseApiFunction<MakeBookingRequest, MakeBookingResponse>(validator, userContextProvider, logger, metricsRecorder)
{
    [OpenApiOperation(operationId: "MakeBooking", tags: ["Booking"], Summary = "Make a booking")]
    [OpenApiRequestBody("application/json", typeof(MakeBookingRequest), Required = true)]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, "application/json", typeof(MakeBookingResponse),
        Description = "Booking successfully made")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.BadRequest, "application/json",
        typeof(IEnumerable<ErrorMessageResponseItem>), Description = "The body of the request is invalid")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.NotFound, "application/json", typeof(string),
        Description = "Requested site not configured for appointments")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, "application/json",
        typeof(ErrorMessageResponseItem), Description = "Unauthorized request to a protected API")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.Forbidden, "application/json", typeof(ErrorMessageResponseItem),
        Description = "Request failed due to insufficient permissions")]
    [RequiresPermission(Permissions.MakeBooking, typeof(SiteFromBodyInspector))]
    [Function("MakeBookingFunction")]
    public override Task<IActionResult> RunAsync(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "booking")]
        HttpRequest req)
    {
        return base.RunAsync(req);
    }

    protected override async Task<ApiResult<MakeBookingResponse>> HandleRequest(MakeBookingRequest bookingRequest,
        ILogger logger)
    {
        var requestedBooking = new Booking
        {
            From = bookingRequest.From,
            Duration = bookingRequest.Duration,
            Service = bookingRequest.Service,
            Site = bookingRequest.Site,
            Status = MapAppointmentStatus(bookingRequest.Kind),
            AttendeeDetails = bookingRequest.AttendeeDetails,
            ContactDetails = bookingRequest.ContactDetails?.ToArray(),
            AdditionalData = bookingRequest.AdditionalData
        };

        var site = await siteService.GetSiteByIdAsync(requestedBooking.Site);
        if (site == null)
        {
            return Failed(HttpStatusCode.NotFound, "Site for booking request could not be found");
        }

        var bookingResult = await bookingWriteService.MakeBooking(requestedBooking);

        if (bookingResult.Success == false)
        {
            return Failed(HttpStatusCode.NotFound, "The time slot for this booking is not available");
        }

        var response = new MakeBookingResponse(bookingResult.Reference);
        return Success(response);
    }

    private AppointmentStatus MapAppointmentStatus(BookingKind kind) => kind switch
    {
        BookingKind.Provisional => AppointmentStatus.Provisional,
        BookingKind.Booked => AppointmentStatus.Booked,
        _ => throw new ArgumentOutOfRangeException(nameof(kind))
    };
}
