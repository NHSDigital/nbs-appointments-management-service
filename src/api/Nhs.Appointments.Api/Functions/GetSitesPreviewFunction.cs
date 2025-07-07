using System.Collections.Generic;
using System.Linq;
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

public class GetSitesPreviewFunction(
    ISiteService siteService,
    IUserService userService,
    IValidator<EmptyRequest> validator,
    IUserContextProvider userContextProvider,
    ILogger<GetSitesPreviewFunction> logger,
    IMetricsRecorder metricsRecorder,
    IPermissionChecker permissionChecker,
    IWellKnowOdsCodesService wellKnowOdsCodesService)
    : BaseApiFunction<EmptyRequest, IEnumerable<SitePreview>>(validator, userContextProvider, logger, metricsRecorder)
{
    [OpenApiOperation(operationId: "GetSitesPreview", tags: ["Site"],
        Summary = "Gets preview of sites available to user")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, "application/json", typeof(IEnumerable<SitePreview>),
        Description = "Users preview sites")]
    [Function("GetSitesPreviewFunction")]
    public override Task<IActionResult> RunAsync(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "sites-preview")]
        HttpRequest req)
    {
        return base.RunAsync(req);
    }

    protected override async Task<ApiResult<IEnumerable<SitePreview>>> HandleRequest(EmptyRequest request,
        ILogger logger)
    {
        var userEmail = Principal.Claims.GetUserEmail();

        var user = await userService.GetUserAsync(userEmail);
        if (user is null)
        {
            return Success([]);
        }

        var sitesResult = new List<SitePreview>();
        var ICBs = (await wellKnowOdsCodesService.GetWellKnownOdsCodeEntries()).Where(x => x.Type == "icb");

        if (await permissionChecker.HasGlobalPermissionAsync(user.Id, Permissions.ViewSitePreview))
        {
            var allSites = await siteService.GetSitesPreview();
            
            foreach (var site in allSites)
            {
                var siteIcb = ICBs.FirstOrDefault(x => x.OdsCode == site.IntegratedCareBoard);
                sitesResult.Add(new SitePreview(site.Id, site.Name, site.OdsCode, siteIcb?.DisplayName));
            }
        }
        else
        {
            var siteIds = await permissionChecker.GetSitesWithPermissionAsync(user.Id, Permissions.ViewSitePreview);

            foreach (var siteId in siteIds)
            {
                var siteInfo = await siteService.GetSiteByIdAsync(siteId);
                if (siteInfo != null)
                {
                    var siteIcb = ICBs.FirstOrDefault(x => x.OdsCode == siteInfo.IntegratedCareBoard);
                    sitesResult.Add(new SitePreview(siteInfo.Id, siteInfo.Name, siteInfo.OdsCode, siteIcb?.DisplayName));
                }
            }

            var regionPermissions = await permissionChecker.GetRegionPermissionsAsync(userEmail);
            if (regionPermissions.Any())
            {
                foreach (var region in regionPermissions.Select(r => r.Replace("region:", "")))
                {
                    var sites = await siteService.GetSitesInRegion(region);

                    sitesResult.AddRange(sites.Select(s =>
                    {
                        var siteIcb = ICBs.FirstOrDefault(x => x.OdsCode == s.IntegratedCareBoard);
                        return new SitePreview(s.Id, s.Name, s.OdsCode, siteIcb?.DisplayName);
                    }));
                }
            }
        }

        return ApiResult<IEnumerable<SitePreview>>.Success(sitesResult.Distinct());
    }

    protected override Task<IEnumerable<ErrorMessageResponseItem>> ValidateRequest(EmptyRequest request)
    {
        return Task.FromResult(Enumerable.Empty<ErrorMessageResponseItem>());
    }
}
