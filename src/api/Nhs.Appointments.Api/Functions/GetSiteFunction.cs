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

public class GetSiteFunction(ISiteService siteService, IValidator<SiteBasedResourceRequest> validator, IUserContextProvider userContextProvider, ILogger<GetSiteFunction> logger) : SiteBasedResourceFunction<Site>(validator, userContextProvider, logger)
{
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
