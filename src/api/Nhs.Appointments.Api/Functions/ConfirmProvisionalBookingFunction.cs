using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
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
using Nhs.Appointments.Core;
using Nhs.Appointments.Core.Inspectors;

namespace Nhs.Appointments.Api.Functions;

public class ConfirmProvisionalBookingFunction(IBookingsService bookingService,
        IValidator<ConfirmBookingRequest> validator,
        IUserContextProvider userContextProvider,
        ILogger<MakeBookingFunction> logger,
        IMetricsRecorder metricsRecorder) : BaseApiFunction<ConfirmBookingRequest, EmptyResponse>(validator, userContextProvider, logger, metricsRecorder)
{        
    [OpenApiOperation(operationId: "ConfirmProvisionalBooking", tags: ["Booking"], Summary = "Confirm a provisional booking")]
    [OpenApiParameter("bookingReference", Required = true, In = ParameterLocation.Path, Description = "The booking reference of the provisional booking")]
    [OpenApiRequestBody("application/json", typeof(ConfirmBookingRequestPayload), Required = false)]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, "application/json", typeof(EmptyResponse), Description = "Returns 200 OK if booking was confirmed")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.BadRequest, "application/json", typeof(IEnumerable<ErrorMessageResponseItem>), Description = "The body of the request is invalid")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, "application/json", typeof(ErrorMessageResponseItem), Description = "Unauthorized request to a protected API")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.Forbidden, "application/json", typeof(ErrorMessageResponseItem), Description = "Request failed due to insufficient permissions")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.Gone, "application/json", typeof(ErrorMessageResponseItem), Description = "Request failed because the provisional booking has expired")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.NotFound, "application/json", typeof(ErrorMessageResponseItem), Description = "Request failed because a provisional booking with matching reference could not be found")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.Processing, "application/json", typeof(ErrorMessageResponseItem), Description = "Request failed because a child booking failed but the lead booking was confirmed")]
    [RequiresPermission(Permissions.MakeBooking, typeof(SiteFromPathInspector))]
    [Function("ConfirmProvisionalBookingFunction")]
    public override Task<IActionResult> RunAsync(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "booking/{bookingReference}/confirm")] HttpRequest req)
    {
        return base.RunAsync(req);
    }

    protected override async Task<ApiResult<EmptyResponse>> HandleRequest(ConfirmBookingRequest bookingRequest, ILogger logger)
    {
        var result = await bookingService.ConfirmProvisionalBooking(bookingRequest.bookingReference,
            bookingRequest.contactDetails.Select(x => new ContactItem { Type = x.Type, Value = x.Value }),
            leadBooker: bookingRequest.childBookings.Length > 0 ? bookingRequest.bookingReference : null, 
            bookingToReschedule: bookingRequest.bookingToReschedule);

        var childResults = new List<BookingConfirmationResult>();

        if (bookingRequest.childBookings.Length > 0 && result == BookingConfirmationResult.Success) 
        {
            foreach (var reference in bookingRequest.childBookings) 
            {
                var childResult = await bookingService.ConfirmProvisionalBooking(reference,
                    bookingRequest.contactDetails.Select(x => new ContactItem { Type = x.Type, Value = x.Value }),
                    leadBooker: bookingRequest.bookingReference, null);

                childResults.Add(childResult);
            }
        }

        switch (result)
        {
            case BookingConfirmationResult.NotFound:
                return Failed(HttpStatusCode.NotFound, "The booking was not found");
            case BookingConfirmationResult.Expired:
                return Failed(HttpStatusCode.Gone, "The provisional booking expired");
            case BookingConfirmationResult.RescheduleNotFound:
                return Failed(HttpStatusCode.NotFound, "The booking to reschedule was not found");
            case BookingConfirmationResult.RescheduleMismatch:
                return Failed(HttpStatusCode.PreconditionFailed,
                    "The nhs number for the provisional booking and the booking to be rescheduled do not match");
            case BookingConfirmationResult.StatusMismatch:
                return Failed(HttpStatusCode.PreconditionFailed,
                    "The booking cannot be confirmed because it is not provisional");
            case BookingConfirmationResult.Success:
                if (childResults.Any(cr => cr != BookingConfirmationResult.Success)) 
                {
                    return Failed(HttpStatusCode.Processing, "Lead booking has persisted but one or more child bookings failed");
                }

                return Success();
        }

        return Failed(HttpStatusCode.InternalServerError, "An unknown error occured when trying to confirm the appointment");
    }

    protected override async Task<(IReadOnlyCollection<ErrorMessageResponseItem> errors, ConfirmBookingRequest request)> ReadRequestAsync(HttpRequest req)
    {
        var contactDetails = new ContactItem[] { };
        var bookingToReschedule = string.Empty;
        var childBookings = Array.Empty<string>();
        if (req.Body != null && req.Body.Length > 0)
        {
            var (errors, payload) = await JsonRequestReader.ReadRequestAsync<ConfirmBookingRequestPayload>(req.Body, true);
            if (errors.Any())
                return (errors, null);
            contactDetails = payload?.contactDetails ?? Array.Empty<ContactItem>();
            bookingToReschedule = payload.bookingToReschedule ?? string.Empty;
            childBookings = payload.childBookings ?? Array.Empty<string>();

            var payloadErrors = new List<ErrorMessageResponseItem>();
            if(payload?.contactDetails == null && payload.bookingToReschedule == null)
            {
                payloadErrors.Add(new ErrorMessageResponseItem { Message = "Request was not valid" });
                return (payloadErrors, null);
            }

        }
        var bookingReference = req.HttpContext.GetRouteValue("bookingReference")?.ToString();

        return (ErrorMessageResponseItem.None, new ConfirmBookingRequest(bookingReference, contactDetails, childBookings, bookingToReschedule));
    }
    
}
