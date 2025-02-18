using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Nhs.Appointments.Api.Availability;
using Nhs.Appointments.Core;
using System.Collections.Concurrent;
using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Nhs.Appointments.Api.Models;
using Nhs.Appointments.Api.Auth;
using Nhs.Appointments.Core.Inspectors;
using Microsoft.AspNetCore.Routing;
using Nhs.Appointments.Api.Json;

namespace Nhs.Appointments.Api.Functions;

public class QueryAvailabilityFunction(IAvailabilityCalculator availabilityCalculator, IValidator<QueryAvailabilityRequest> validator, IAvailabilityGrouperFactory availabilityGrouperFactory, IUserContextProvider userContextProvider, ILogger<QueryAvailabilityFunction> logger, IMetricsRecorder metricsRecorder)
    : BaseApiFunction<QueryAvailabilityRequest, QueryAvailabilityResponse>(validator, userContextProvider, logger, metricsRecorder)
{
    [OpenApiOperation(operationId: "QueryAvailability", tags: ["Availability"], Summary = "Query appointment availability by days, hours or slots")]
    [OpenApiRequestBody("application/json", typeof(QueryAvailabilityRequest),Required = true)]
    [OpenApiResponseWithBody(statusCode:HttpStatusCode.OK, "application/json", typeof(QueryAvailabilityResponseItem[]), Description = "Appointment availability")]
    [OpenApiResponseWithBody(statusCode:HttpStatusCode.BadRequest, "application/json", typeof(IEnumerable<ErrorMessageResponseItem>),  Description = "The body of the request is invalid" )]
    [OpenApiResponseWithBody(statusCode:HttpStatusCode.Unauthorized, "application/json", typeof(ErrorMessageResponseItem), Description = "Unauthorized request to a protected API")]
    [OpenApiResponseWithBody(statusCode:HttpStatusCode.Forbidden, "application/json", typeof(ErrorMessageResponseItem), Description = "Request failed due to insufficient permissions")]
    [RequiresPermission(Permissions.QueryAvailability, typeof(MultiSiteBodyRequestInspector))]
    [Function("QueryAvailabilityFunction")]
    public override Task<IActionResult> RunAsync(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "availability/query")] HttpRequest req)
    {
        return base.RunAsync(req);
    }

    protected override async Task<ApiResult<QueryAvailabilityResponse>> HandleRequest(QueryAvailabilityRequest request, ILogger logger)
    {
        var concurrentResults = new ConcurrentBag<QueryAvailabilityResponseItem>();
        var response = new QueryAvailabilityResponse();
        var requestFrom = request.From;
        var requestUntil = request.Until;
        var requestConsecutive = request.Consecutive;

        await Parallel.ForEachAsync(request.Sites, async (site, ct) =>
        {
            var siteAvailability = await GetAvailability(site, request.Service, request.QueryType, requestFrom, requestUntil, requestConsecutive);
            concurrentResults.Add(siteAvailability);
        });

        response.AddRange(concurrentResults.Where(r => r is not null).OrderBy(r => r.site));
        return Success(response);
    }

    private async Task<QueryAvailabilityResponseItem> GetAvailability(string site, string service, QueryType queryType, DateOnly from, DateOnly until, int consecutive)
    {
        var slots = (await availabilityCalculator.CalculateAvailability(site, service, from, until)).GroupByConsecutive(consecutive).ToList();        
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
        var request = default(QueryAvailabilityRequest);
        var service = string.Empty;
        if (req.Body != null && req.Body.Length > 0)
        {
            var (errors, payload) = await JsonRequestReader.ReadRequestAsync<QueryAvailabilityRequest>(req.Body, true);
            if (errors.Any())
                return (errors, null);

            request = payload;
        }
        var bookingReference = req.HttpContext.GetRouteValue("bookingReference")?.ToString();

        return (ErrorMessageResponseItem.None, new QueryAvailabilityRequest(request.Sites, request.Service, request.From, request.Until, request.QueryType, request.Consecutive <= 0 ? 1 : request.Consecutive));
    }
}
