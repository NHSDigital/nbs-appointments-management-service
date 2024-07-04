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

public class SetBookingStatusFunction : BaseApiFunction<SetBookingStatusRequest, SetBookingStatusResponse>
{
    private readonly IBookingsService _bookingService;

    public SetBookingStatusFunction(
        IBookingsService bookingService,
        IValidator<SetBookingStatusRequest> validator,
        IUserContextProvider userContextProvider, 
        ILogger<SetBookingStatusFunction> logger) : base(validator, userContextProvider, logger)
    {
        _bookingService = bookingService;
    }

    [OpenApiOperation(operationId: "SetBookingStatus", tags: new [] {"Booking"}, Summary = "Set the status of a booking")]
    [OpenApiRequestBody("text/json", typeof(SetBookingStatusRequest))]
    [OpenApiSecurity("Api Key", SecuritySchemeType.ApiKey, Name = "Authorization", In = OpenApiSecurityLocationType.Header)]
    [OpenApiResponseWithBody(statusCode:HttpStatusCode.OK, "text/plain", typeof(SetBookingStatusResponse), Description = "Booking status successfully set")]
    [OpenApiResponseWithBody(statusCode:HttpStatusCode.NotFound, "text/json", typeof(ApiResult<object>), Description = "Booking not found")]
    [RequiresPermission("booking:set-status")]
    [Function("SetBookingStatusFunction")]
    public override Task<IActionResult> RunAsync(
       [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "booking/set-status")] HttpRequest req)
    {
        return base.RunAsync(req);
    }

    protected override async Task<ApiResult<SetBookingStatusResponse>> HandleRequest(SetBookingStatusRequest request, ILogger logger)
    {
        var result = await _bookingService.SetBookingStatus(request.bookingReference, request.status);
        return result ?
            Success(new SetBookingStatusResponse(request.bookingReference, request.status)):
            Failed(System.Net.HttpStatusCode.NotFound, $"Booking {request.bookingReference} not found");
    }
}