using System.Collections.Generic;
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

namespace Nhs.Appointments.Api.Functions;

public class GetSitesByAreaFunction(ISiteService siteService, IValidator<GetSitesByAreaRequest> validator, IUserContextProvider userContextProvider, ILogger<GetSitesByAreaFunction> logger)
    : BaseApiFunction<GetSitesByAreaRequest, IEnumerable<SiteWithDistance>>(validator, userContextProvider, logger)
{
    [OpenApiOperation(operationId: "GetSitesByArea", tags: new [] {"Sites"}, Summary = "Get sites by area")]
    [OpenApiSecurity("Api Key", SecuritySchemeType.ApiKey, Name = "Authorization", In = OpenApiSecurityLocationType.Header)]
    [OpenApiResponseWithBody(statusCode:HttpStatusCode.OK, "text/plain", typeof(IEnumerable<SiteWithDistance>), Description = "List of sites in requested area with distance from requested area")]
    [OpenApiResponseWithBody(statusCode:HttpStatusCode.BadRequest, contentType: "text/json", typeof(IEnumerable<ErrorMessageResponseItem>),  Description = "The body of the request is invalid" )]
    [RequiresPermission("sites:query", typeof(NoSiteRequestInspector))]
    [Function("GetSitesByAreaFunction")]
    public override Task<IActionResult> RunAsync([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "sites")] HttpRequest req)
    {
        return base.RunAsync(req);
    }

    protected override async Task<ApiResult<IEnumerable<SiteWithDistance>>> HandleRequest(GetSitesByAreaRequest byAreaRequest, ILogger logger)
    {
        var sites = await siteService.FindSitesByArea(byAreaRequest.longitude, byAreaRequest.latitude, byAreaRequest.searchRadius, byAreaRequest.maximumRecords);
        return ApiResult<IEnumerable<SiteWithDistance>>.Success(sites);
    }
    
    protected override Task<(bool requestRead, GetSitesByAreaRequest request)> ReadRequestAsync(HttpRequest req)
    {
        var longitude = double.Parse(req.Query["long"]);
        var latitude = double.Parse(req.Query["lat"]);
        var searchRadius = int.Parse(req.Query["searchRadius"]);
        var maximumRecords = int.Parse(req.Query["maxRecords"]);
        return Task.FromResult<(bool requestRead, GetSitesByAreaRequest request)>((true, new GetSitesByAreaRequest(longitude, latitude, searchRadius, maximumRecords)));
    }
}