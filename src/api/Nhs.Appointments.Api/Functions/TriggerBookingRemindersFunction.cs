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

public class TriggerBookingRemindersFunction(
    IBookingsService bookingService,
    IValidator<EmptyRequest> validator,
    IUserContextProvider userContextProvider,
    ILogger<TriggerBookingRemindersFunction> logger,
    IMetricsRecorder metricsRecorder)
    : BaseApiFunction<EmptyRequest, EmptyResponse>(validator, userContextProvider, logger, metricsRecorder)
{
    [OpenApiOperation(operationId: "TriggerBookingReminders", tags: ["System"],
        Summary = "Utility function to manually trigger reminder notifications for bookings")]
    [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.OK,
        Description = "Reminder notifications manually triggered")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, "application/json",
        typeof(ErrorMessageResponseItem), Description = "Unauthorized request to a protected API")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.Forbidden, "application/json", typeof(ErrorMessageResponseItem),
        Description = "Request failed due to insufficient permissions")]
    [RequiresPermission(Permissions.SystemRunReminders, typeof(NoSiteRequestInspector))]
    [Function("TriggerBookingReminders")]
    public override Task<IActionResult> RunAsync(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "system/run-reminders")]
        HttpRequest req)
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
