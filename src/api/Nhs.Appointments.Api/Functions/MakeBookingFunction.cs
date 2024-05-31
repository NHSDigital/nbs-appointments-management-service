using System;
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
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.OpenApi.Models;
using Nhs.Appointments.Api.Auth;

namespace Nhs.Appointments.Api.Functions;

public class MakeBookingFunction : BaseApiFunction<MakeBookingRequest, MakeBookingResponse>
{    
    private readonly IBookingsService _bookingService;
    private readonly ISiteConfigurationService _siteConfigurationService;
    private readonly IAvailabilityCalculator _availabilityCalculator;

    public MakeBookingFunction(
        IBookingsService bookingService,         
        ISiteConfigurationService siteConfigurationService,
        IAvailabilityCalculator availabilityCalculator,
        IValidator<MakeBookingRequest> validator,
        IUserContextProvider userContextProvider, 
        ILogger<MakeBookingFunction> logger) : base(validator, userContextProvider, logger)
    {
        _bookingService = bookingService;
        _siteConfigurationService = siteConfigurationService;
        _availabilityCalculator = availabilityCalculator;
    }

    [OpenApiOperation(operationId: "MakeBooking", tags: new [] {"Booking"}, Summary = "Make a booking")]
    [OpenApiRequestBody("text/json", typeof(MakeBookingRequest))]
    [OpenApiSecurity("Api Key", SecuritySchemeType.ApiKey, Name = "Authorization", In = OpenApiSecurityLocationType.Header)]
    [OpenApiResponseWithBody(statusCode:HttpStatusCode.OK, "text/plain", typeof(MakeBookingResponse), Description = "Booking successfully made")]
    [OpenApiResponseWithBody(statusCode:HttpStatusCode.BadRequest, contentType: "text/json", typeof(IEnumerable<ErrorMessageResponseItem>),  Description = "The body of the request is invalid" )]
    [OpenApiResponseWithBody(statusCode:HttpStatusCode.NotFound, "text/plain", typeof(string), Description = "Requested site not configured for appointments")]
    [Function("MakeBookingFunction")]
    public override Task<IActionResult> RunAsync(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "booking")] HttpRequest req)
    {
        return base.RunAsync(req);
    }

    protected override async Task<ApiResult<MakeBookingResponse>> HandleRequest(MakeBookingRequest bookingRequest, ILogger logger)
    {
        var siteConfigurationOp = await TryPattern.TryAsync(() => _siteConfigurationService.GetSiteConfigurationAsync(bookingRequest.Site));
        if (siteConfigurationOp.Completed == false)
            return Failed(HttpStatusCode.NotFound, "The requested site is not configured for appointments");

        var selectedServiceConfiguration = siteConfigurationOp.Result.ServiceConfiguration.SingleOrDefault(app => app.Code == bookingRequest.Service);

        if (selectedServiceConfiguration == null)
            return Failed(HttpStatusCode.NotFound, "The requested service is not available");

        var duration = selectedServiceConfiguration.Duration;
        var bookingDate = DateOnly.FromDateTime(bookingRequest.FromDateTime);

        var requestedBooking = new Booking
        {
            From = bookingRequest.FromDateTime,
            Duration = duration,
            SessionHolder = bookingRequest.SessionHolder,
            Service = bookingRequest.Service,
            Site = bookingRequest.Site,
            AttendeeDetails = new Core.AttendeeDetails
            {
                DateOfBirth = bookingRequest.AttendeeDetails.BirthDate,
                FirstName = bookingRequest.AttendeeDetails.FirstName,
                LastName = bookingRequest.AttendeeDetails.LastName,
                NhsNumber = bookingRequest.AttendeeDetails.NhsNumber
            }
        };

        var blocks = await _availabilityCalculator.CalculateAvailability(bookingRequest.Site, bookingRequest.Service, bookingDate,
            bookingDate.AddDays(1));
        var canBook = blocks.Any(bl => bl.SessionHolder == requestedBooking.SessionHolder && bl.Contains(requestedBooking.TimePeriod));

        if (canBook)
        {
            var bookingResult = await _bookingService.MakeBooking(requestedBooking);
            if (bookingResult.Success == false)
                return Failed(HttpStatusCode.InternalServerError, "Unable to make booking");

            var response = new MakeBookingResponse(bookingResult.Reference);
            return Success(response);
        }

        return Failed(HttpStatusCode.NotFound, "The time slot for this booking is not available");
    }       
}