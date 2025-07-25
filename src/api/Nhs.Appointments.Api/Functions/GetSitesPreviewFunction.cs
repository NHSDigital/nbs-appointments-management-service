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
        
        var icbs = (await wellKnowOdsCodesService.GetWellKnownOdsCodeEntries()).Where(x => x.Type == "icb");

        var sites = await permissionChecker.GetSitesWithPermissionAsync(user.Id, Permissions.ViewSitePreview);
        var siteResults = sites.Select(site => 
            new SitePreview(
                site.Id, 
                site.Name, 
                site.OdsCode,
                icbs.FirstOrDefault(x => x.OdsCode.Equals(site.IntegratedCareBoard))?.DisplayName ??
            site.IntegratedCareBoard));
        
        return ApiResult<IEnumerable<SitePreview>>.Success(siteResults.Distinct());
    }

    protected override Task<IEnumerable<ErrorMessageResponseItem>> ValidateRequest(EmptyRequest request)
    {
        return Task.FromResult(Enumerable.Empty<ErrorMessageResponseItem>());
    }
}
