using System.Collections.Generic;
using System.Linq;
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

public class MakeBookingFunction : BaseApiFunction<MakeBookingRequest, MakeBookingResponse>
{    
    private readonly IBookingsService _bookingService;    
    private readonly IAvailabilityCalculator _availabilityCalculator;

    public MakeBookingFunction(
        IBookingsService bookingService,         
        IAvailabilityCalculator availabilityCalculator,
        IValidator<MakeBookingRequest> validator,
        IUserContextProvider userContextProvider, 
        ILogger<MakeBookingFunction> logger) : base(validator, userContextProvider, logger)
    {
        _bookingService = bookingService;
        _availabilityCalculator = availabilityCalculator;
    }

    [OpenApiOperation(operationId: "MakeBooking", tags: new [] {"Booking"}, Summary = "Make a booking")]
    [OpenApiRequestBody("text/json", typeof(MakeBookingRequest))]
    [OpenApiResponseWithBody(statusCode:HttpStatusCode.OK, "text/plain", typeof(MakeBookingResponse), Description = "Booking successfully made")]
    [OpenApiResponseWithBody(statusCode:HttpStatusCode.BadRequest, contentType: "text/json", typeof(IEnumerable<ErrorMessageResponseItem>),  Description = "The body of the request is invalid" )]
    [OpenApiResponseWithBody(statusCode:HttpStatusCode.NotFound, "text/plain", typeof(string), Description = "Requested site not configured for appointments")]
    [RequiresPermission("booking:make", typeof(SiteFromBodyInspector))]
    [Function("MakeBookingFunction")]
    public override Task<IActionResult> RunAsync(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "booking")] HttpRequest req)
    {
        return base.RunAsync(req);
    }

    protected override async Task<ApiResult<MakeBookingResponse>> HandleRequest(MakeBookingRequest bookingRequest, ILogger logger)
    {
        var requestedBooking = new Booking
        {
            From = bookingRequest.FromDateTime,
            Duration = bookingRequest.Duration,
            Service = bookingRequest.Service,
            Site = bookingRequest.Site,
            AttendeeDetails = new Core.AttendeeDetails
            {
                DateOfBirth = bookingRequest.AttendeeDetails.BirthDate,
                FirstName = bookingRequest.AttendeeDetails.FirstName,
                LastName = bookingRequest.AttendeeDetails.LastName,
                NhsNumber = bookingRequest.AttendeeDetails.NhsNumber
            },
            ContactDetails = bookingRequest.ContactDetails?.Select(c => new Core.ContactItem { Type = c.Type, Value = c.Value}).ToArray()
        };

        
        var bookingResult = await _bookingService.MakeBooking(requestedBooking);
        if (bookingResult.Success == false)
            return Failed(HttpStatusCode.NotFound, "The time slot for this booking is not available");

        var response = new MakeBookingResponse(bookingResult.Reference);
        return Success(response);               
    }    
}