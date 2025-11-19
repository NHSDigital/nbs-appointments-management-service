using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Extensions.Logging;
using Nhs.Appointments.Api.Auth;
using Nhs.Appointments.Api.Availability;
using Nhs.Appointments.Api.Models;
using Nhs.Appointments.Core.Availability;
using Nhs.Appointments.Core.Bookings;
using Nhs.Appointments.Core.Inspectors;
using Nhs.Appointments.Core.Sites;
using Nhs.Appointments.Core.Users;
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
    ISiteService siteService
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
    public override Task<IActionResult> RunAsync(HttpRequest req)
    {
        return base.RunAsync(req);
    }

    protected override async Task<ApiResult<List<AvailabilityByDays>>> HandleRequest(AvailabilityQueryRequest request, ILogger logger)
    {
        var sites = await siteService.GetAllSites();
        var activeSites = request.Sites.Where(rs => sites.Any(s => s.Id == rs && s.isDeleted is false or null));
        if (!activeSites.Any())
        {
            return Success([]);
        }

        return Success([]);
    }
}
