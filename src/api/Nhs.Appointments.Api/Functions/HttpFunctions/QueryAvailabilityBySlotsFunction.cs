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
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Nhs.Appointments.Api.Functions.HttpFunctions;
public class QueryAvailabilityBySlotsFunction(
    IBookingAvailabilityStateService bookingAvailabilityStateService,
    IValidator<AvailabilityQueryBySlotsRequest> validator,
    IUserContextProvider userContextProvider,
    ILogger<QueryAvailabilityBySlotsFunction> logger,
    IMetricsRecorder metricsRecorder,
    IAvailableSlotsFilter availableSlotsFilter,
    ISiteService siteService,
    IFeatureToggleHelper featureToggleHelper) : BaseApiFunction<AvailabilityQueryBySlotsRequest, AvailabilityBySlots>(validator, userContextProvider, logger, metricsRecorder)
{
    [OpenApiOperation(operationId: "QueryAvailabilityBySlots", tags: ["Availability"],
        Summary = "Query appointment availability by slts")]
    [OpenApiRequestBody("application/json", typeof(AvailabilityQueryBySlotsRequest), Required = true)]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, "application/json", typeof(AvailabilityBySlots),
        Description = "Appointment availability")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.BadRequest, "application/json",
        typeof(IEnumerable<ErrorMessageResponseItem>), Description = "The body of the request is invalid")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, "application/json",
        typeof(ErrorMessageResponseItem), Description = "Unauthorized request to a protected API")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.Forbidden, "application/json", typeof(ErrorMessageResponseItem),
        Description = "Request failed due to insufficient permissions")]
    [RequiresPermission(Permissions.QueryAvailability, typeof(MultiSiteBodyRequestInspector))]
    [Function("QueryAvailabilityBySlotsFunction")]
    public override async Task<IActionResult> RunAsync(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "availability/query/slots")]
        HttpRequest req)
    {
        return await featureToggleHelper.IsFeatureEnabled(Flags.MultiServiceJointBookings)
            ? await base.RunAsync(req)
            : ProblemResponse(HttpStatusCode.NotImplemented, null);
    }

    protected async override Task<ApiResult<AvailabilityBySlots>> HandleRequest(AvailabilityQueryBySlotsRequest request, ILogger logger)
    {
        if (await siteService.GetSiteByIdAsync(request.Site) is null)
        {
            return Failed(HttpStatusCode.NotFound, $"Site: {request.Site} could not be found.");
        }

        var slots = (await bookingAvailabilityStateService.GetAvailableSlots(request.Site, request.From, request.Until)).ToList();
        var filteredSlots = availableSlotsFilter.FilterAvailableSlots(slots, request.Attendees);

        return Success(AvailabilityGrouper.BuildSlotsAvailability(request.Site, request.From, request.Until, request.Attendees, filteredSlots));
    }
}
