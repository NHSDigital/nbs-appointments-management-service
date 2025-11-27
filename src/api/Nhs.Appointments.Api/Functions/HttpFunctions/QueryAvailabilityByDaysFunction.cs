using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Extensions.Logging;
using Nhs.Appointments.Api.Auth;
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
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Nhs.Appointments.Api.Functions.HttpFunctions;

public class QueryAvailabilityByDaysFunction(
    IBookingAvailabilityStateService bookingAvailabilityStateService,
    IValidator<AvailabilityQueryRequest> validator,
    IUserContextProvider userContextProvider,
    ILogger<QueryAvailabilityByDaysFunction> logger,
    IMetricsRecorder metricsRecorder,
    IAvailableSlotsFilter availableSlotsFilter,
    ISiteService siteService,
    IFeatureToggleHelper featureToggleHelper
    ) : BaseApiFunction<AvailabilityQueryRequest, List<AvailabilityByDays>>(validator, userContextProvider, logger, metricsRecorder)
{
    [OpenApiOperation(operationId: "QueryAvailabilityByDays", tags: ["Availability"],
        Summary = "Query appointment availability by days")]
    [OpenApiRequestBody("application/json", typeof(AvailabilityQueryRequest), Required = true)]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, "application/json", typeof(List<AvailabilityByDays>),
        Description = "Appointment availability")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.BadRequest, "application/json",
        typeof(IEnumerable<ErrorMessageResponseItem>), Description = "The body of the request is invalid")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, "application/json",
        typeof(ErrorMessageResponseItem), Description = "Unauthorized request to a protected API")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.Forbidden, "application/json", typeof(ErrorMessageResponseItem),
        Description = "Request failed due to insufficient permissions")]
    [RequiresPermission(Permissions.QueryAvailability, typeof(MultiSiteBodyRequestInspector))]
    [Function("QueryAvailabilityByDaysFunction")]
    public override async Task<IActionResult> RunAsync(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "availability/query/days")]
        HttpRequest req)
    {
        return await featureToggleHelper.IsFeatureEnabled(Flags.MultiServiceJointBookings)
            ? await base.RunAsync(req)
            : ProblemResponse(HttpStatusCode.NotImplemented, null);
    }

    protected override async Task<ApiResult<List<AvailabilityByDays>>> HandleRequest(AvailabilityQueryRequest request, ILogger logger)
    {
        var sites = await siteService.GetAllSites();
        var activeSites = request.Sites.Where(rs => sites.Any(s => s.Id == rs));
        if (!activeSites.Any())
        {
            return Success([]);
        }

        var concurrentResults = new ConcurrentBag<AvailabilityByDays>();
        await Parallel.ForEachAsync(activeSites, async (site, ct) =>
        {
            var siteAvailability = await GetSiteAvailability(site, request.Attendees, request.From, request.Until);
            concurrentResults.Add(siteAvailability);
        });

        var response = new List<AvailabilityByDays>();
        response.AddRange(concurrentResults.Where(r => r is not null));

        return Success(response);
    }

    private async Task<AvailabilityByDays> GetSiteAvailability(string site, List<Attendee> attendees, DateOnly from, DateOnly until)
    {
        var dayStart = from.ToDateTime(new TimeOnly(0, 0));
        var dayEnd = until.ToDateTime(new TimeOnly(23, 59, 59));

        var slots = (await bookingAvailabilityStateService.GetAvailableSlots(site, dayStart, dayEnd)).ToList();
        var filteredSlots = availableSlotsFilter.FilterAvailableSlots(slots, attendees);

        var dayEntries = new List<DayEntry>();

        var day = from;
        while (day <= until)
        {
            var slotsForDay = filteredSlots.Where(s => day == DateOnly.FromDateTime(s.From));
            if (slotsForDay.Any())
            {
                dayEntries.Add(AvailabilityGrouper.BuildDayAvailability(day, slotsForDay));
            }

            day = day.AddDays(1);
        }

        return new AvailabilityByDays
        {
            Site = site,
            Days = dayEntries
        };
    }
}
