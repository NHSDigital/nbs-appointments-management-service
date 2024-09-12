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

public class GetSiteFunction(ISiteService siteService, IValidator<SiteBasedResourceRequest> validator, IUserContextProvider userContextProvider, ILogger<GetSiteFunction> logger) : SiteBasedResourceFunction<Site>(validator, userContextProvider, logger)
{
    [OpenApiOperation(operationId: "GetSite", tags: new [] {"Site"}, Summary = "Get single site by Id")]
    [OpenApiSecurity("Api Key", SecuritySchemeType.ApiKey, Name = "Authorization", In = OpenApiSecurityLocationType.Header)]
    [OpenApiResponseWithBody(statusCode:HttpStatusCode.OK, "text/plain", typeof(AttributeSets), Description = "Information for a single site")]
    [OpenApiResponseWithBody(statusCode:HttpStatusCode.Forbidden, "text/plain", typeof(ErrorMessageResponseItem), Description = "Request failed due to insufficient permissions")]
    [OpenApiResponseWithBody(statusCode:HttpStatusCode.Unauthorized, "text/plain", typeof(ErrorMessageResponseItem), Description = "Unauthorized request to a protected API")]
    [RequiresPermission("site:get-config", typeof(SiteFromPathInspector))]
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
        return Failed(System.Net.HttpStatusCode.NotFound, "The specified site was not found.");
    }
};
