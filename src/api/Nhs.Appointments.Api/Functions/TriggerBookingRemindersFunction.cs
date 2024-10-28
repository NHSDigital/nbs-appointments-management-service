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

public class TriggerBookingRemindersFunction(IBookingsService bookingService, IValidator<EmptyRequest> validator, IUserContextProvider userContextProvider, ILogger<TriggerBookingRemindersFunction> logger, IMetricsRecorder metricsRecorder) : BaseApiFunction<EmptyRequest, EmptyResponse>(validator, userContextProvider, logger, metricsRecorder)
{

    [OpenApiOperation(operationId: "TriggerBookingReminders", tags: ["System", "Booking"], Summary = "Manually trigger reminder notifications for bookings")]
    [OpenApiSecurity("Api Key", SecuritySchemeType.ApiKey, Name = "Authorization", In = OpenApiSecurityLocationType.Header)]
    [RequiresPermission("system:run-reminders", typeof(NoSiteRequestInspector))]
    [Function("TriggerBookingReminders")]
    public override Task<IActionResult> RunAsync(
       [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "system/run-reminders")] HttpRequest req)
    {
        return base.RunAsync(req);
    }

    protected override async Task<ApiResult<EmptyResponse>> HandleRequest(EmptyRequest request, ILogger logger)
    {
        await bookingService.SendBookingReminders();
        return Success(new EmptyResponse());
    }

    protected override Task<IEnumerable<ErrorMessageResponseItem>> ValidateRequest(EmptyRequest request)
    {
        return Task.FromResult(Enumerable.Empty<ErrorMessageResponseItem>());
    }
}


