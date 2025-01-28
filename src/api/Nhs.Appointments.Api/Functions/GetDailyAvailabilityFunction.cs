using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Nhs.Appointments.Api.Auth;
using Nhs.Appointments.Api.Models;
using Nhs.Appointments.Core;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.OpenApi.Models;
using Nhs.Appointments.Core.Inspectors;

namespace Nhs.Appointments.Api.Functions;

public class GetDailyAvailabilityFunction(IAvailabilityService availabilityService, IValidator<GetDailyAvailabilityRequest> validator, IUserContextProvider userContextProvider, ILogger<GetDailyAvailabilityFunction> logger, IMetricsRecorder metricsRecorder)
    : BaseApiFunction<GetDailyAvailabilityRequest, IEnumerable<DailyAvailability>>(validator, userContextProvider, logger, metricsRecorder)
{
    [OpenApiOperation(operationId: "Get_DailyAvailabilityFunction", tags: ["Availability"], Summary = "Get daily availability within a date range previously")]
    [OpenApiParameter("site", In = ParameterLocation.Query, Required = true, Type = typeof(double), Description = "The id of them site where to get daily availability from")]
    [OpenApiParameter("from", In = ParameterLocation.Query, Required = true, Type = typeof(double), Description = "The start of the date range to get daily availability")]
    [OpenApiParameter("to", In = ParameterLocation.Query, Required = true, Type = typeof(double), Description = "The end of the date range to get daily availability")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, "application/json", typeof(IEnumerable<DailyAvailability>), Description = "List of daily availability within a date range")]
    [OpenApiResponseWithBody(statusCode:HttpStatusCode.Unauthorized, "application/json", typeof(ErrorMessageResponseItem), Description = "Unauthorized request to a protected API")]
    [OpenApiResponseWithBody(statusCode:HttpStatusCode.Forbidden, "application/json", typeof(ErrorMessageResponseItem), Description = "Request failed due to insufficient permissions")]
    [RequiresPermission(Roles.AvailabilityQuery, typeof(SiteFromQueryStringInspector))]
    [Function("GetDailyAvailabilityFunction")]
    public override Task<IActionResult> RunAsync([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "daily-availability")] HttpRequest req)
    {
        return base.RunAsync(req);
    }

    protected override async Task<ApiResult<IEnumerable<DailyAvailability>>> HandleRequest(GetDailyAvailabilityRequest request, ILogger logger)
    {
        var availability = await availabilityService.GetDailyAvailability(request.Site, request.FromDate, request.UntilDate);

        return Success(availability);
    }

    protected override Task<(IReadOnlyCollection<ErrorMessageResponseItem> errors, GetDailyAvailabilityRequest request)> ReadRequestAsync(HttpRequest req)
    {
        var errors = new List<ErrorMessageResponseItem>();

        var site = req.Query["site"];
        var from = req.Query["from"];
        var until = req.Query["until"];

        return Task.FromResult<(IReadOnlyCollection<ErrorMessageResponseItem> errors, GetDailyAvailabilityRequest request)>
            ((errors.AsReadOnly(), new GetDailyAvailabilityRequest(site, from, until)));
    }
}
