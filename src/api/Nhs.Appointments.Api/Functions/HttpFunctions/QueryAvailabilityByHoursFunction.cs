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
public class QueryAvailabilityByHoursFunction(
    IBookingAvailabilityStateService bookingAvailabilityStateService,
    IValidator<AvailabilityQueryByHoursRequest> validator,
    IUserContextProvider userContextProvider,
    ILogger<QueryAvailabilityByHoursFunction> logger,
    IMetricsRecorder metricsRecorder,
    IAvailableSlotsFilter availableSlotsFilter,
    ISiteService siteService,
    IFeatureToggleHelper featureToggleHelper) : BaseApiFunction<AvailabilityQueryByHoursRequest, AvailabilityByHours>(validator, userContextProvider, logger, metricsRecorder)
{
    [OpenApiOperation(operationId: "QueryAvailabilityByHours", tags: ["Availability"],
        Summary = "Query appointment availability by hours")]
    [OpenApiRequestBody("application/json", typeof(AvailabilityQueryRequest), Required = true)]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, "application/json", typeof(List<AvailabilityByHours>),
        Description = "Appointment availability")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.BadRequest, "application/json",
        typeof(IEnumerable<ErrorMessageResponseItem>), Description = "The body of the request is invalid")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, "application/json",
        typeof(ErrorMessageResponseItem), Description = "Unauthorized request to a protected API")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.Forbidden, "application/json", typeof(ErrorMessageResponseItem),
        Description = "Request failed due to insufficient permissions")]
    [RequiresPermission(Permissions.QueryAvailability, typeof(MultiSiteBodyRequestInspector))]
    [Function("QueryAvailabilityByHoursFunction")]
    public override async Task<IActionResult> RunAsync(HttpRequest req)
    {
        return await featureToggleHelper.IsFeatureEnabled(Flags.MultiServiceJointBookings)
            ? await base.RunAsync(req)
            : ProblemResponse(HttpStatusCode.NotImplemented, null);
    }

    protected override async Task<ApiResult<AvailabilityByHours>> HandleRequest(AvailabilityQueryByHoursRequest request, ILogger logger)
    {
        return await siteService.GetSiteByIdAsync(request.Site) is null
            ? Failed(HttpStatusCode.NotFound, $"Site: {request.Site} could not be found.")
            : Success(await GetSiteAvailability(request.Site, request.Attendees, request.From, request.Until));
    }

    private async Task<AvailabilityByHours> GetSiteAvailability(string site, List<Attendee> attendees, DateOnly from, DateOnly until)
    {
        var dayStart = from.ToDateTime(new TimeOnly(0, 0));
        var dayEnd = until.ToDateTime(new TimeOnly(23, 59, 59));

        var slots = (await bookingAvailabilityStateService.GetAvailableSlots(site, dayStart, dayEnd)).ToList();
        var filteredSlots = availableSlotsFilter.FilterAvailableSlots(slots, attendees);

        return AvailabilityGrouper.BuildHourAvailability(site, DateOnly.FromDateTime(dayStart), attendees, filteredSlots);
    }
}
