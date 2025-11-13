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
using Nhs.Appointments.Core.Bookings;
using Nhs.Appointments.Core.Inspectors;
using Nhs.Appointments.Core.Users;

namespace Nhs.Appointments.Api.Functions.HttpFunctions;

public class TriggerUnconfirmedProvisionalBookingsCollectorFunction(
    IBookingWriteService bookingWriteService,
    IValidator<RemoveExpiredProvisionalBookingsRequest> validator,
    IUserContextProvider userContextProvider,
    ILogger<TriggerBookingRemindersFunction> logger,
    IMetricsRecorder metricsRecorder)
    : BaseApiFunction<RemoveExpiredProvisionalBookingsRequest, RemoveExpiredProvisionalBookingsResponse>(validator, userContextProvider, logger,
        metricsRecorder)
{
    [OpenApiOperation(operationId: "TriggerUnconfirmedProvisionalBookingsCollector", tags: ["System"],
        Summary = "Utility function to manually trigger the removal of expired unconfirmed provisional bookings")]
    [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.OK, Description = "Expired provisional bookings removed")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, "application/json",
        typeof(ErrorMessageResponseItem), Description = "Unauthorized request to a protected API")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.Forbidden, "application/json", typeof(ErrorMessageResponseItem),
        Description = "Request failed due to insufficient permissions")]
    [RequiresPermission(Permissions.SystemRunProvisionalSweeper, typeof(NoSiteRequestInspector))]
    [Function("TriggerUnconfirmedProvisionalBookingsCollector")]
    public override Task<IActionResult> RunAsync(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "system/run-provisional-sweep")]
        HttpRequest req)
    {
        return base.RunAsync(req);
    }

    protected override async Task<ApiResult<RemoveExpiredProvisionalBookingsResponse>> HandleRequest(
        RemoveExpiredProvisionalBookingsRequest request, ILogger logger)
    {
        var batchSize = request.BatchSize
         ?? int.Parse(Environment.GetEnvironmentVariable("CleanupBatchSize") ?? "200");

        var degreeOfParallelism = request.DegreeOfParallelism
            ?? int.Parse(Environment.GetEnvironmentVariable("CleanupDegreeOfParallelism") ?? "8");

        var removedIds = await bookingWriteService.RemoveUnconfirmedProvisionalBookings(batchSize, degreeOfParallelism);
        return Success(new RemoveExpiredProvisionalBookingsResponse(removedIds.ToArray()));
    }

    protected override Task<IEnumerable<ErrorMessageResponseItem>> ValidateRequest(RemoveExpiredProvisionalBookingsRequest request)
    {
        return Task.FromResult(Enumerable.Empty<ErrorMessageResponseItem>());
    }
}
