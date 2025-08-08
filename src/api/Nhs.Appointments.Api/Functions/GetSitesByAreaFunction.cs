using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Nhs.Appointments.Api.Auth;
using Nhs.Appointments.Api.Models;
using Nhs.Appointments.Core;
using Nhs.Appointments.Core.Inspectors;

namespace Nhs.Appointments.Api.Functions;

public class GetSitesByAreaFunction(
    ISiteService siteService,
    IValidator<GetSitesByAreaRequest> validator,
    IUserContextProvider userContextProvider,
    ILogger<GetSitesByAreaFunction> logger,
    IMetricsRecorder metricsRecorder)
    : BaseApiFunction<GetSitesByAreaRequest, IEnumerable<SiteWithDistance>>(validator, userContextProvider, logger,
        metricsRecorder)
{
    [OpenApiOperation(operationId: "GetSitesByArea", tags: ["Sites"],
        Summary = "Get sites within a specified area from a given location")]
    [OpenApiParameter("longitude", In = ParameterLocation.Query, Required = true, Type = typeof(double),
        Description = "The longitude value of the search location")]
    [OpenApiParameter("latitude", In = ParameterLocation.Query, Required = true, Type = typeof(double),
        Description = "The latitude value of the search location")]
    [OpenApiParameter("searchRadius", In = ParameterLocation.Query, Required = true, Type = typeof(int),
        Description =
            "The radius distance in meters used to search for sites within the radius from the given location")]
    [OpenApiParameter("maximumRecords", In = ParameterLocation.Query, Required = true, Type = typeof(int),
        Description = "The maximum number of sites to return from the query")]
    [OpenApiParameter("accessNeeds", In = ParameterLocation.Query, Required = false, Type = typeof(string[]),
        CollectionDelimiter = OpenApiParameterCollectionDelimiterType.Comma,
        Description = "Required access needs used to filter sites")]
    [OpenApiParameter("services", In = ParameterLocation.Query, Required = false, Type = typeof(string[]),
        CollectionDelimiter = OpenApiParameterCollectionDelimiterType.Comma,
        Description =
            "Optional parameter to be used alongside from and until. When all three of these parameters are used, it filters the results to only those sites that support the services within that date range")]
    [OpenApiParameter("from", In = ParameterLocation.Query, Required = false, Type = typeof(string),
        Description =
            "Optional parameter to be used alongside services and until. When all three of these parameters are used, it filters the results to only those sites that support the services within that date range. DateFormat yyyy-MM-dd.")]
    [OpenApiParameter("until", In = ParameterLocation.Query, Required = false, Type = typeof(string),
        Description =
            "Optional parameter to be used alongside services and from. When all three of these parameters are used, it filters the results to only those sites that support the services within that date range. DateFormat yyyy-MM-dd.")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, "application/json", typeof(IEnumerable<SiteWithDistance>),
        Description = "List of sites within a geographical area that support requested access needs")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.BadRequest, "application/json",
        typeof(IEnumerable<ErrorMessageResponseItem>), Description = "The body of the request is invalid")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, "application/json",
        typeof(ErrorMessageResponseItem), Description = "Unauthorized request to a protected API")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.Forbidden, "application/json", typeof(ErrorMessageResponseItem),
        Description = "Request failed due to insufficient permissions")]
    [RequiresPermission(Permissions.QuerySites, typeof(NoSiteRequestInspector))]
    [Function("GetSitesByAreaFunction")]
    public override Task<IActionResult> RunAsync(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "sites")]
        HttpRequest req)
    {
        return base.RunAsync(req);
    }

    protected override async Task<ApiResult<IEnumerable<SiteWithDistance>>> HandleRequest(GetSitesByAreaRequest request,
        ILogger logger)
    {
        SiteSupportsServiceFilter siteSupportsServiceFilter = null;

        //should pass validation rules
        if (request.services is { Length: 1 } && request.from != null && request.until != null)
        {
            var fromDate = DateOnly.ParseExact(request.from, "yyyy-MM-dd");
            var untilDate = DateOnly.ParseExact(request.until, "yyyy-MM-dd");
            siteSupportsServiceFilter = new SiteSupportsServiceFilter(request.services.Single(), fromDate, untilDate);
        }

        var sites = await siteService.FindSitesByArea(request.longitude, request.latitude, request.searchRadius,
            request.maximumRecords, request.accessNeeds, request.ignoreCache, siteSupportsServiceFilter);
        return ApiResult<IEnumerable<SiteWithDistance>>.Success(sites);
    }

    protected override Task<(IReadOnlyCollection<ErrorMessageResponseItem> errors, GetSitesByAreaRequest request)>
        ReadRequestAsync(HttpRequest req)
    {
        var errors = new List<ErrorMessageResponseItem>();
        var accessNeeds = req.Query.ContainsKey("accessNeeds")
            ? req.Query["accessNeeds"].ToString().Split(',')
            : Array.Empty<string>();
        var ignoreCache = req.Query.ContainsKey("ignoreCache") ? bool.Parse(req.Query["ignoreCache"]) : false;
        if (accessNeeds.Any(string.IsNullOrEmpty))
        {
            errors.Add(new ErrorMessageResponseItem
            {
                Property = "accessNeeds", Message = "Access needs cannot be contain empty values"
            });
            return Task
                .FromResult<(IReadOnlyCollection<ErrorMessageResponseItem> errors, GetSitesByAreaRequest request)>((
                    errors.AsReadOnly(), null));
        }

        var longitude = double.Parse(req.Query["long"]);
        var latitude = double.Parse(req.Query["lat"]);
        var searchRadius = int.Parse(req.Query["searchRadius"]);
        var maximumRecords = int.Parse(req.Query["maxRecords"]);

        var services = req.Query.ContainsKey("services")
            ? req.Query["services"].ToString().Split(',')
            : null;

        var from = req.Query.ContainsKey("from")
            ? req.Query["from"].ToString()
            : null;

        var until = req.Query.ContainsKey("until")
            ? req.Query["until"].ToString()
            : null;

        return Task.FromResult<(IReadOnlyCollection<ErrorMessageResponseItem> errors, GetSitesByAreaRequest request)>((
            errors.AsReadOnly(),
            new GetSitesByAreaRequest(longitude, latitude, searchRadius, maximumRecords, accessNeeds, ignoreCache,
                services, from, until)));
    }
}
