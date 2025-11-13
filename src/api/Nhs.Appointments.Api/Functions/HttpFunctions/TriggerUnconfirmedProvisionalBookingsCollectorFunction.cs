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
using Nhs.Appointments.Api.Auth;
using Nhs.Appointments.Api.Json;
using Nhs.Appointments.Api.Models;
using Nhs.Appointments.Core.Bookings;
using Nhs.Appointments.Core.Inspectors;
using Nhs.Appointments.Core.Sites;
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
        request ??= new RemoveExpiredProvisionalBookingsRequest(null, null);

        var batchSize = request.BatchSize
            ?? (int.TryParse(Environment.GetEnvironmentVariable("CleanupBatchSize"), out var bs) ? bs : 200);

        var degreeOfParallelism = request.DegreeOfParallelism
            ?? (int.TryParse(Environment.GetEnvironmentVariable("CleanupDegreeOfParallelism"), out var dop) ? dop : 8);

        var removedIds = await bookingWriteService.RemoveUnconfirmedProvisionalBookings(batchSize, degreeOfParallelism);
        return Success(new RemoveExpiredProvisionalBookingsResponse(removedIds.ToArray()));
    }

    protected override async Task<(IReadOnlyCollection<ErrorMessageResponseItem> errors, RemoveExpiredProvisionalBookingsRequest request)> ReadRequestAsync(HttpRequest req)
    {
        if (req.ContentLength == 0)
        {
            return (Array.Empty<ErrorMessageResponseItem>(), new RemoveExpiredProvisionalBookingsRequest(null, null));
        }

        if (req.ContentLength.HasValue && req.ContentLength.Value > 0)
        {
            return await base.ReadRequestAsync(req).ConfigureAwait(false);
        }

        var buffer = new System.IO.MemoryStream();
        await req.Body.CopyToAsync(buffer).ConfigureAwait(false);
        buffer.Position = 0;

        string bodyText;
        using (var sr = new System.IO.StreamReader(buffer, System.Text.Encoding.UTF8, detectEncodingFromByteOrderMarks: true, leaveOpen: true))
        {
            bodyText = await sr.ReadToEndAsync().ConfigureAwait(false);
        }

        buffer.Position = 0;
        req.Body = buffer;

        if (string.IsNullOrWhiteSpace(bodyText))
        {
            return (Array.Empty<ErrorMessageResponseItem>(), new RemoveExpiredProvisionalBookingsRequest(null, null));
        }

        return await base.ReadRequestAsync(req).ConfigureAwait(false);
    }


}
