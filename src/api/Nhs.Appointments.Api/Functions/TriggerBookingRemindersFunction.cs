using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Nhs.Appointments.Api.Auth;
using Nhs.Appointments.Api.Models;
using Nhs.Appointments.Core;

namespace Nhs.Appointments.Api.Functions;

public class TriggerBookingRemindersFunction : BaseApiFunction<EmptyRequest, EmptyResult>
{
    private readonly IBookingsService _bookingService;

    public TriggerBookingRemindersFunction(IBookingsService bookingService, IValidator<EmptyRequest> validator, IUserContextProvider userContextProvider, ILogger<TriggerBookingRemindersFunction> logger) :
        base(validator, userContextProvider, logger)
    {
        _bookingService = bookingService;
    }

    [OpenApiOperation(operationId: "TriggerBookingReminders", tags: ["System", "Booking"], Summary = "Manually trigger reminder notifications for bookings")]
    [OpenApiSecurity("Api Key", SecuritySchemeType.ApiKey, Name = "Authorization", In = OpenApiSecurityLocationType.Header)]
    [RequiresPermission("system:run-reminders", typeof(NoSiteRequestInspector))]
    [Function("TriggerBookingReminders")]
    public override Task<IActionResult> RunAsync(
       [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "system/run-reminders")] HttpRequest req)
    {
        return base.RunAsync(req);
    }

    protected async override Task<ApiResult<EmptyResult>> HandleRequest(EmptyRequest request, ILogger logger)
    {
        await _bookingService.SendBookingReminders();
        return new ApiResult<EmptyResult>();
    }

    protected override Task<IEnumerable<ErrorMessageResponseItem>> ValidateRequest(EmptyRequest request)
    {
        return Task.FromResult(Enumerable.Empty<ErrorMessageResponseItem>());
    }
}


