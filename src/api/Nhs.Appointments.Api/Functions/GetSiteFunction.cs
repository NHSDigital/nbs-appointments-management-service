using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Extensions.Logging;
using Nhs.Appointments.Api.Auth;
using Nhs.Appointments.Api.Models;
using Nhs.Appointments.Core;

namespace Nhs.Appointments.Api.Functions;

public class GetSiteFunction(ISiteService siteService, IValidator<SiteBasedResourceRequest> validator, IUserContextProvider userContextProvider, ILogger<GetSiteFunction> logger) : SiteBasedResourceFunction<Site>(validator, userContextProvider, logger)
{
    [OpenApiOperation(operationId: "GetSite", tags: ["Sites"], Summary = "Get single site by Id")]
    [OpenApiResponseWithBody(statusCode:HttpStatusCode.OK, "text/plain", typeof(Site), Description = "Information for a single site")]
    [OpenApiResponseWithBody(statusCode:HttpStatusCode.NotFound, "text/json", typeof(ApiResult<object>), Description = "Site not found")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.BadRequest, contentType: "text/json", typeof(IEnumerable<ErrorMessageResponseItem>), Description = "The body of the request is invalid")]
    [OpenApiResponseWithBody(statusCode:HttpStatusCode.Forbidden, "text/plain", typeof(ErrorMessageResponseItem), Description = "Request failed due to insufficient permissions")]
    [OpenApiResponseWithBody(statusCode:HttpStatusCode.Unauthorized, "text/plain", typeof(ErrorMessageResponseItem), Description = "Unauthorized request to a protected API")]
    [RequiresPermission("site:view", typeof(SiteFromPathInspector))]
    [Function("GetSiteFunction")]
    public override Task<IActionResult> RunAsync([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "sites/{site}")] HttpRequest req)
    {
        return base.RunAsync(req);
    }

    protected override async Task<ApiResult<Site>> HandleRequest(SiteBasedResourceRequest request, ILogger logger)
    {
        var site = await siteService.GetSiteByIdAsync(request.Site);
        if (site != null)
        {            
            return ApiResult<Site>.Success(site);
        }  
        return Failed(HttpStatusCode.NotFound, "The specified site was not found.");
    }
};
