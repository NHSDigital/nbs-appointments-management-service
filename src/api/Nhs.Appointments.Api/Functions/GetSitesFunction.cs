using System.Collections.Generic;
using System.Threading.Tasks;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Nhs.Appointments.Api.Auth;
using Nhs.Appointments.Api.Models;
using Nhs.Appointments.Core;

namespace Nhs.Appointments.Api.Functions;

public class GetSitesFunction(ISiteSearchService siteSearchService, IValidator<GetSitesRequest> validator, IUserContextProvider userContextProvider, ILogger<GetSitesFunction> logger)
    : BaseApiFunction<GetSitesRequest, IEnumerable<SiteWithDistance>>(validator, userContextProvider, logger)
{
    [Function("GetSitesFunction")]
    public override Task<IActionResult> RunAsync([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "sites")] HttpRequest req)
    {
        return base.RunAsync(req);
    }

    protected override async Task<ApiResult<IEnumerable<SiteWithDistance>>> HandleRequest(GetSitesRequest request, ILogger logger)
    {
        var sites = await siteSearchService.FindSitesByArea(request.longitude, request.latitude, request.searchRadius, request.maximumRecords);
        return ApiResult<IEnumerable<SiteWithDistance>>.Success(sites);
    }
    
    protected override Task<(bool requestRead, GetSitesRequest request)> ReadRequestAsync(HttpRequest req)
    {
        var longitude = double.Parse(req.Query["long"]);
        var latitude = double.Parse(req.Query["lat"]);
        var searchRadius = int.Parse(req.Query["searchRadius"]);
        var maximumRecords = int.Parse(req.Query["maxRecords"]);
        return Task.FromResult<(bool requestRead, GetSitesRequest request)>((true, new GetSitesRequest(longitude, latitude, searchRadius, maximumRecords)));
    }
}