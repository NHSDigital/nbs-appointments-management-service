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

namespace Nhs.Appointments.Api.Functions;

public class QueryAvailabilityFunction(IAvailabilityCalculator availabilityCalculator, IValidator<QueryAvailabilityRequest> validator, IAvailabilityGrouperFactory availabilityGrouperFactory, IUserContextProvider userContextProvider, ILogger<QueryAvailabilityFunction> logger)
    : BaseApiFunction<QueryAvailabilityRequest, QueryAvailabilityResponse>(validator, userContextProvider, logger)
{

    [OpenApiOperation(operationId: "QueryAvailability", tags: ["Availability"], Summary = "Query appointment availability by Days, Hours or Slots")]
    [OpenApiRequestBody("text/json", typeof(QueryAvailabilityRequest),Required = true)]
    [OpenApiResponseWithBody(statusCode:HttpStatusCode.OK, contentType: "text/json", typeof(QueryAvailabilityRequest), Description = "Appointment availability")]
    [OpenApiResponseWithBody(statusCode:HttpStatusCode.BadRequest, contentType: "text/json", typeof(IEnumerable<ErrorMessageResponseItem>),  Description = "The body of the request is invalid" )]
    [RequiresPermission("availability:query", typeof(NoSiteRequestInspector))]
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
        var requestFrom = request.FromDate;
        var requestUntil = request.UntilDate;

        await Parallel.ForEachAsync(request.Sites, async (site, ct) =>
        {
            var siteAvailability = await GetAvailability(site, request.Service, request.QueryType, requestFrom, requestUntil);
            concurrentResults.Add(siteAvailability);
        });

        response.AddRange(concurrentResults.Where(r => r is not null).OrderBy(r => r.site));
        return Success(response);
    }

    private async Task<QueryAvailabilityResponseItem> GetAvailability(string site, string service, QueryType queryType, DateOnly from, DateOnly until)
    {
        var slots = (await availabilityCalculator.CalculateAvailability(site, service, from, until)).ToList();        
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
}