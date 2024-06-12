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
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.OpenApi.Models;
using Nhs.Appointments.Api.Models;
using Nhs.Appointments.Api.Auth;

namespace Nhs.Appointments.Api.Functions;

public class QueryAvailabilityFunction : BaseApiFunction<QueryAvailabilityRequest, QueryAvailabilityResponse>
{
    private readonly ISiteConfigurationService _siteConfigurationService;
    private readonly IAvailabilityCalculator _availabilityCalculator;
    private readonly IAvailabilityGrouperFactory _availabilityGrouperFactory;
    
    public QueryAvailabilityFunction(
        IAvailabilityCalculator availabilityCalculator, 
        ISiteConfigurationService siteConfigurationService,
        IValidator<QueryAvailabilityRequest> validator,
        IAvailabilityGrouperFactory availabilityGrouperFactory,
        IUserContextProvider userContextProvider,
        ILogger<QueryAvailabilityFunction> logger) : base(validator, userContextProvider, logger)
    {
        _availabilityCalculator = availabilityCalculator;
        _siteConfigurationService = siteConfigurationService;
        _availabilityGrouperFactory = availabilityGrouperFactory;
    }

    [OpenApiOperation(operationId: "QueryAvailability", tags: new [] {"Appointment Availability"}, Summary = "Query appointment availability")]
    [OpenApiRequestBody("text/json", typeof(QueryAvailabilityRequest),Required = true)]
    [OpenApiSecurity("Api Key", SecuritySchemeType.ApiKey, Name = "Authorization", In = OpenApiSecurityLocationType.Header)]
    [OpenApiResponseWithBody(statusCode:HttpStatusCode.OK, contentType: "text/json", typeof(QueryAvailabilityRequest), Description = "Appointment availability")]
    [OpenApiResponseWithBody(statusCode:HttpStatusCode.BadRequest, contentType: "text/json", typeof(IEnumerable<ErrorMessageResponseItem>),  Description = "The body of the request is invalid" )]
    [RequiresPermission("availability:query")]
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
        var blocks = (await _availabilityCalculator.CalculateAvailability(site, service, from, until)).ToList();

        var siteConfigurationOp = await TryPattern.TryAsync(() => _siteConfigurationService.GetSiteConfigurationAsync(site));
        if (siteConfigurationOp.Completed == false)
            return null;

        var selectedServiceConfiguration = siteConfigurationOp.Result.ServiceConfiguration.SingleOrDefault(app => app.Code == service);
        if (selectedServiceConfiguration is null)
            return null;

        var availability = new List<QueryAvailabilityResponseInfo>();

        var day = from;
        while (day <= until)
        {
            var blocksForDay = blocks.Where(b => day == DateOnly.FromDateTime(b.From));
            var availabilityGrouper = _availabilityGrouperFactory.Create(queryType);
            var groupedBlocks = availabilityGrouper.GroupAvailability(blocksForDay, selectedServiceConfiguration.Duration);
            availability.Add(new QueryAvailabilityResponseInfo(day, groupedBlocks));
            day = day.AddDays(1);
        }

        return new QueryAvailabilityResponseItem(site, service, availability);
    }
}