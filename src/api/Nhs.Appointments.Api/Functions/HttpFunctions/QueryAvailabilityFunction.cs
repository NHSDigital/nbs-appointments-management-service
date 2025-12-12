using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Extensions.Logging;
using Nhs.Appointments.Api.Auth;
using Nhs.Appointments.Api.Availability;
using Nhs.Appointments.Api.Json;
using Nhs.Appointments.Api.Models;
using Nhs.Appointments.Core.Availability;
using Nhs.Appointments.Core.Bookings;
using Nhs.Appointments.Core.Features;
using Nhs.Appointments.Core.Inspectors;
using Nhs.Appointments.Core.Sites;
using Nhs.Appointments.Core.Users;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Nhs.Appointments.Api.Functions.HttpFunctions;

public class QueryAvailabilityFunction(
    IBookingAvailabilityStateService bookingAvailabilityStateService,
    IValidator<QueryAvailabilityRequest> validator,
    IAvailabilityGrouperFactory availabilityGrouperFactory,
    IUserContextProvider userContextProvider,
    ILogger<QueryAvailabilityFunction> logger,
    IMetricsRecorder metricsRecorder,
    IFeatureToggleHelper featureToggleHelper,
    IHasConsecutiveCapacityFilter hasConsecutiveCapacityFilter,
    ISiteService siteService)
    : BaseApiFunction<QueryAvailabilityRequest, QueryAvailabilityResponse>(validator, userContextProvider, logger,
        metricsRecorder)
{
    [OpenApiOperation(operationId: "QueryAvailability", tags: ["Availability"],
        Summary = "Query appointment availability by days, hours or slots")]
    [OpenApiRequestBody("application/json", typeof(QueryAvailabilityRequest), Required = true)]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, "application/json", typeof(QueryAvailabilityResponseItem[]),
        Description = "Appointment availability")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.BadRequest, "application/json",
        typeof(IEnumerable<ErrorMessageResponseItem>), Description = "The body of the request is invalid")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, "application/json",
        typeof(ErrorMessageResponseItem), Description = "Unauthorized request to a protected API")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.Forbidden, "application/json", typeof(ErrorMessageResponseItem),
        Description = "Request failed due to insufficient permissions")]
    [RequiresPermission(Permissions.QueryAvailability, typeof(MultiSiteBodyRequestInspector))]
    [Function("QueryAvailabilityFunction")]
    public override Task<IActionResult> RunAsync(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "availability/query")]
        HttpRequest req)
    {
        return base.RunAsync(req);
    }

    protected override async Task<ApiResult<QueryAvailabilityResponse>> HandleRequest(QueryAvailabilityRequest request,
        ILogger logger)
    {
        var response = new QueryAvailabilityResponse();
        var requestFrom = request.From;
        var requestUntil = request.Until;
        var requestConsecutive = request.Consecutive;

        var sites = await siteService.GetAllSites();
        var activeSites = request.Sites.Where(rs => sites.Any(s => s.Id == rs && s.isDeleted is false or null)).ToArray();
        if (!activeSites.Any())
        {
            return Success([]);
        }

        var concurrentResults = await Task.WhenAll(activeSites.Select(async site =>
        {
            Stopwatch sw = new();
            sw.Start();
            
            var siteAvailability = await GetAvailability(site, request.Service, request.QueryType, requestFrom, requestUntil, requestConsecutive ?? 1);
            
            sw.Stop();
            logger.LogInformation($"result: {sw.Elapsed}");

            return siteAvailability;
        }));

        response.AddRange(concurrentResults.Where(r => r is not null).OrderBy(r => r.site));
        return Success(response);
    }

    private async Task<QueryAvailabilityResponseItem> GetAvailability(string site, string service, QueryType queryType, DateOnly from, DateOnly until, int consecutive)
    {
        var dayStart = from.ToDateTime(new TimeOnly(0, 0));
        var dayEnd = until.ToDateTime(new TimeOnly(23, 59, 59));

        var slots = (await bookingAvailabilityStateService.GetAvailableSlots(site, dayStart, dayEnd))
                .Where(x => x.Services.Contains(service)).ToArray();
        
        if (await featureToggleHelper.IsFeatureEnabled(Flags.JointBookings)) 
        {
            slots = await hasConsecutiveCapacityFilter.SessionHasConsecutiveSessions(slots, consecutive);
        }

        var availability = new List<QueryAvailabilityResponseInfo>();

        var day = from;
        while (day <= until)
        {
            var slotsForDay = slots.Where(b => day == DateOnly.FromDateTime(b.From));
            var availabilityGrouper = availabilityGrouperFactory.Create(queryType);
            var groupedBlocks = availabilityGrouper.GroupAvailability(slotsForDay);
            availability.Add(new QueryAvailabilityResponseInfo(day, groupedBlocks));
            day = day.AddDays(1);
        }

        return new QueryAvailabilityResponseItem(site, service, availability);
    }

    protected override async Task<(IReadOnlyCollection<ErrorMessageResponseItem> errors, QueryAvailabilityRequest request)> ReadRequestAsync(HttpRequest req)
    {
        if (req.Body.Length > 0)
        {
            var (errors, payload) = await JsonRequestReader.ReadRequestAsync<QueryAvailabilityRequest>(req.Body, true);
            if (errors.Any())
                return (errors, null);
            
            return (ErrorMessageResponseItem.None,
                payload with { Consecutive = payload.Consecutive ?? 1 });
        }
        
        return (new[] { new ErrorMessageResponseItem { Message = "Request is empty"}}, null);
    }
}
