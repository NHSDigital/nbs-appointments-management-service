using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Extensions.Logging;
using Nhs.Appointments.Api.Models;
using Nhs.Appointments.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Nhs.Appointments.Api.Functions;
public class GetSitesPreviewFunction(ISiteService siteService, IUserService userService, IValidator<EmptyRequest> validator, IUserContextProvider userContextProvider, ILogger<GetSitesPreviewFunction> logger, IMetricsRecorder metricsRecorder)
    : BaseApiFunction<EmptyRequest, IEnumerable<SitePreview>>(validator, userContextProvider, logger, metricsRecorder)
{



    [OpenApiOperation(operationId: "GetSitesPreview", tags: ["Site"], Summary = "Gets preview of sites available to user")] 
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, "application/json", typeof(IEnumerable<SitePreview>), Description = "Users preview sites")]
    [Function("GetSitesPreviewFunction")]
    public override Task<IActionResult> RunAsync(
      [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "sites-preview")] HttpRequest req)
    {
        return base.RunAsync(req);
    }

    protected override async Task<ApiResult<IEnumerable<SitePreview>>> HandleRequest(EmptyRequest request, ILogger logger)
    {
        var userEmail = Principal.Claims.GetUserEmail();

        var user = await userService.GetUserAsync(userEmail);
        if (user is null)
        {
            return Success(Enumerable.Empty<SitePreview>());
        }

        var sitesResult = new List<SitePreview>();

        if (IsAdminUser(user))
        {
            var allSites = await siteService.GetSitesPreview();

            foreach (var site in allSites)
            {
                sitesResult.Add(new SitePreview(site.Id, site.Name));
            }
        }
        else
        {
            var siteIdsForUser = user.RoleAssignments.Where(ra => ra.Scope.StartsWith("site:")).Select(ra => ra.Scope.Replace("site:", ""));

            foreach (var site in siteIdsForUser.Distinct())
            {
                var siteInfo = await siteService.GetSiteByIdAsync(site);
                if (siteInfo != null)
                    sitesResult.Add(new SitePreview(siteInfo.Id, siteInfo.Name));
            }
        }

        return ApiResult<IEnumerable<SitePreview>>.Success(sitesResult);
    }

    protected override Task<IEnumerable<ErrorMessageResponseItem>> ValidateRequest(EmptyRequest request)
    {
        return Task.FromResult(Enumerable.Empty<ErrorMessageResponseItem>());
    }

    private bool IsAdminUser(User user)
    {
        return user.RoleAssignments.Any(ra => ra.Role == "system:admin-user");
    }
}
