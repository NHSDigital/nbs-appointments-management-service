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

namespace Nhs.Appointments.Api.Functions;

public class ConfirmProvisionalBookingFunction(IBookingsService bookingService,
        IValidator<ConfirmBookingRequest> validator,
        IUserContextProvider userContextProvider,
        ILogger<MakeBookingFunction> logger) : BaseApiFunction<ConfirmBookingRequest, EmptyResponse>(validator, userContextProvider, logger)
{
    
    [OpenApiOperation(operationId: "ConfirmProvisionalBooking", tags: [ "Booking" ], Summary = "Confirm a provisional booking")]
    [OpenApiSecurity("Api Key", SecuritySchemeType.ApiKey, Name = "Authorization", In = OpenApiSecurityLocationType.Header)]
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
}