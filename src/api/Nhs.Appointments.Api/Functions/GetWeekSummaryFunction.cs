using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
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

namespace Nhs.Appointments.Api.Functions;

public class GetWeekSummaryFunction(
    IBookingAvailabilityStateService bookingAvailabilityStateService,
    IFeatureToggleHelper featureToggleHelper,
    IValidator<GetWeekSummaryRequest> validator,
    IUserContextProvider userContextProvider,
    ILogger<GetWeekSummaryFunction> logger,
    IMetricsRecorder metricsRecorder)
    : BaseApiFunction<GetWeekSummaryRequest, WeekSummary>(validator, userContextProvider,
        logger, metricsRecorder)
{
    [OpenApiOperation(operationId: "GetWeekSummary", tags: ["Availability"],
        Summary = "Get weekly availability summary for a week start date and site")]
    [OpenApiParameter("site", In = ParameterLocation.Query, Required = true, Type = typeof(double),
        Description = "The id of them site where to get daily availability from")]
    [OpenApiParameter("from", In = ParameterLocation.Query, Required = true, Type = typeof(double),
        Description = "The start of the week date")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, "application/json", typeof(WeekSummary),
        Description = "A weekly summary for the availability and daily sessions")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, "application/json",
        typeof(ErrorMessageResponseItem), Description = "Unauthorized request to a protected API")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.Forbidden, "application/json", typeof(ErrorMessageResponseItem),
        Description = "Request failed due to insufficient permissions")]
    [RequiresPermission(Permissions.QueryAvailability, typeof(SiteFromQueryStringInspector))]
    [Function("GetWeekSummaryFunction")]
    public override Task<IActionResult> RunAsync(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "week-summary")] HttpRequest req
    )
    {
        return base.RunAsync(req);
    }

    protected override async Task<ApiResult<WeekSummary>> HandleRequest(
        GetWeekSummaryRequest request, ILogger logger)
    {
        if (!await featureToggleHelper.IsFeatureEnabled(Flags.MultipleServices))
        {
            throw new NotImplementedException();
        }
        
        var weekSummary =
            await bookingAvailabilityStateService.GetWeekSummary(request.Site, request.FromDate);

        return Success(weekSummary);
    }

    protected override Task<(IReadOnlyCollection<ErrorMessageResponseItem> errors, GetWeekSummaryRequest request)>
        ReadRequestAsync(HttpRequest req)
    {
        var errors = new List<ErrorMessageResponseItem>();

        var site = req.Query["site"];
        var from = req.Query["from"];

        return Task
            .FromResult<(IReadOnlyCollection<ErrorMessageResponseItem> errors, GetWeekSummaryRequest request)>
                ((errors.AsReadOnly(), new GetWeekSummaryRequest(site, from)));
    }
}
