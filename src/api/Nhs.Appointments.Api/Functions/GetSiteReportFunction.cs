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
using Microsoft.Extensions.Logging;
using Nhs.Appointments.Api.Auth;
using Nhs.Appointments.Api.Models;
using Nhs.Appointments.Core;

namespace Nhs.Appointments.Api.Functions;

public class GetSiteReportFunction(
    IUserService userService,
    IPermissionChecker permissionChecker,
    ISiteService siteService,
    ISiteReportService siteReportService,
    IValidator<SiteReportRequest> validator,
    IUserContextProvider userContextProvider,
    ILogger<GetAccessibilityDefinitionsFunction> logger,
    IMetricsRecorder metricsRecorder)
    : BaseApiFunction<SiteReportRequest, IEnumerable<SiteReport>>(validator, userContextProvider, logger,
        metricsRecorder)
{
    [OpenApiOperation(operationId: "GetDailyReport", tags: ["DailyReport"],
        Summary = "Get Daily Report")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, "application/json",
        typeof(IEnumerable<SiteReport>),
        Description = "Report for all Sites based on a Date Range")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, "application/json",
        typeof(ErrorMessageResponseItem), Description = "Unauthorized request to a protected API")]
    [Function("GetSiteReportFunction")]
    public override Task<IActionResult> RunAsync(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "SiteReport")]
        HttpRequest req)
    {
        return base.RunAsync(req);
    }

    private async Task<IEnumerable<Site>> GetSites(User user)
    {
        var sites = await siteService.GetAllSites();
        
        if (await permissionChecker.HasGlobalPermissionAsync(user.Id, Permissions.ReportSites))
        {
            return sites;
        }
        
        var siteIds = await permissionChecker.GetSitesWithPermissionAsync(user.Id, Permissions.ReportSites);
        var regionPermissions = (await permissionChecker.GetRegionPermissionsAsync(user.Id)).Select(r => r.Replace("region:", ""));
            
        return sites.Where(x => siteIds.Contains(x.Id) || regionPermissions.Contains(x.Region));
    }

    protected override async Task<ApiResult<IEnumerable<SiteReport>>> HandleRequest(SiteReportRequest request,
        ILogger logger)
    {
        var userEmail = Principal.Claims.GetUserEmail();

        var user = await userService.GetUserAsync(userEmail);
        if (user is null)
        {
            return Success([]);
        }

        var sites = await GetSites(user);

        var report = await siteReportService.Generate(sites.ToArray(), request.StartDate, request.EndDate);
        
        return ApiResult<IEnumerable<SiteReport>>.Success(report);
    }
    
    protected override
        Task<(IReadOnlyCollection<ErrorMessageResponseItem> errors, SiteReportRequest request)>
        ReadRequestAsync(HttpRequest req)
    {
        var errors = new List<ErrorMessageResponseItem>();
        req.Query.TryGetValue("startDate", out var startDateInput);
        req.Query.TryGetValue("endDate", out var endDateInput);

        if (!DateOnly.TryParseExact(startDateInput.ToString(), "dd-MM-yyyy", out var startDate))
        {
            errors.Add(new ErrorMessageResponseItem()
            {
                Message = "Start Date is required",
                Property = "startDate"
            });
        }
        
        if (!DateOnly.TryParseExact(endDateInput.ToString(), "dd-MM-yyyy", out var endDate))
        {
            errors.Add(new ErrorMessageResponseItem()
            {
                Message = "End Date is required",
                Property = "endDate"
            });
        }
        
        return Task
            .FromResult<(IReadOnlyCollection<ErrorMessageResponseItem> errors, SiteReportRequest
                request)>((errors, new SiteReportRequest(startDate, endDate)));
    }
}
