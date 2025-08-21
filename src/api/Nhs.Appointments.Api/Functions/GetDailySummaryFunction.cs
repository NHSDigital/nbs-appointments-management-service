using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Nhs.Appointments.Api.Auth;
using Nhs.Appointments.Api.Models;
using Nhs.Appointments.Core;
using Nhs.Appointments.Core.Features;
using Nhs.Appointments.Core.Inspectors;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace Nhs.Appointments.Api.Functions;

public class GetDailySummaryFunction(
    IBookingAvailabilityStateService bookingAvailabilityStateService,
    IValidator<GetDaySummaryRequest> validator,
    IUserContextProvider userContextProvider,
    ILogger<GetDailySummaryFunction> logger,
    IMetricsRecorder metricsRecorder,
    IFeatureToggleHelper featureToggleHelper)
    : BaseApiFunction<GetDaySummaryRequest, Summary>(validator, userContextProvider,
        logger, metricsRecorder)
{
    [OpenApiOperation(operationId: "GetDaySummary", tags: ["Availability"],
        Summary = "Get daily availability summary for a day date and site")]
    [OpenApiParameter("site", In = ParameterLocation.Query, Required = true, Type = typeof(string),
        Description = "The ID of the site from which to query bookings and availability")]
    [OpenApiParameter("from", In = ParameterLocation.Query, Required = true, Type = typeof(double),
        Description = "The date for the selected day")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, "application/json", typeof(Summary),
        Description = "A daily summary for the availability and daily sessions")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, "application/json",
        typeof(ErrorMessageResponseItem), Description = "Unauthorized request to a protected API")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.Forbidden, "application/json", typeof(ErrorMessageResponseItem),
        Description = "Request failed due to insufficient permissions")]
    [RequiresPermission(Permissions.QueryAvailability, typeof(SiteFromQueryStringInspector))]
    [Function("GetDaySummaryFunction")]
    public override Task<IActionResult> RunAsync(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "day-summary")] HttpRequest req
    )
    {
        return base.RunAsync(req);
    }

    protected override async Task<ApiResult<Summary>> HandleRequest(
        GetDaySummaryRequest request, ILogger logger)
    {
        var daySummary =
            await bookingAvailabilityStateService.GetDaySummary(request.Site, request.FromDate);

        return Success(daySummary);
    }

    protected override Task<(IReadOnlyCollection<ErrorMessageResponseItem> errors, GetDaySummaryRequest request)>
        ReadRequestAsync(HttpRequest req)
    {
        var errors = new List<ErrorMessageResponseItem>();

        var site = req.Query["site"];
        var from = req.Query["from"];

        return Task
            .FromResult<(IReadOnlyCollection<ErrorMessageResponseItem> errors, GetDaySummaryRequest request)>
                ((errors.AsReadOnly(), new GetDaySummaryRequest(site, from)));
    }
}
