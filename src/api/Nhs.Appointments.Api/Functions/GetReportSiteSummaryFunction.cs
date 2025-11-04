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
using Nhs.Appointments.Api.File;
using Nhs.Appointments.Api.Models;
using Nhs.Appointments.Core;
using Nhs.Appointments.Core.Features;
using Nhs.Appointments.Core.Inspectors;
using Nhs.Appointments.Core.Reports.SiteSummary;

namespace Nhs.Appointments.Api.Functions;

public class GetReportSiteSummaryFunction(
    IUserService userService,
    IPermissionChecker permissionChecker,
    ISiteReportService siteReportService,
    ISiteReportCsvWriter siteReportCsvWriter,
    IFeatureToggleHelper featureToggleHelper,
    IValidator<SiteReportRequest> validator,
    IUserContextProvider userContextProvider,
    ILogger<GetAccessibilityDefinitionsFunction> logger,
    IMetricsRecorder metricsRecorder)
    : BaseApiFunction<SiteReportRequest, FileResponse>(validator, userContextProvider, logger,
        metricsRecorder)
{
    [OpenApiOperation(operationId: "GetDailyReport", tags: ["DailyReport"],
        Summary = "Get Daily Report")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, "text/csv",
        typeof(IEnumerable<SiteReport>),
        Description = "Report for all Sites based on a Date Range")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, "application/json",
        typeof(ErrorMessageResponseItem), Description = "Unauthorized request to a protected API")]
    [RequiresPermission(Permissions.ReportsSiteSummary, typeof(AnyUserSitesRequestInspector))]
    [Function("GetReportSiteSummaryFunction")]
    public override async Task<IActionResult> RunAsync(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "report/site-summary")]
        HttpRequest req)
    {
        return !await featureToggleHelper.IsFeatureEnabled(Flags.SiteSummaryReport)
            ? ProblemResponse(HttpStatusCode.NotImplemented, "Site Summary Reports are not enabled.")
            : await base.RunAsync(req);
    }

    protected override string ResponseType => ApiResponseType.File;

    protected override async Task<ApiResult<FileResponse>> HandleRequest(SiteReportRequest request,
        ILogger logger)
    {
        var userEmail = Principal.Claims.GetUserEmail();
        var user = await userService.GetUserAsync(userEmail);
        var sites = await permissionChecker.GetSitesWithPermissionAsync(user.Id, Permissions.ReportsSiteSummary);

        var siteReports = await siteReportService.GenerateReports(sites.ToArray(), request.StartDate, request.EndDate);

        var csv = await siteReportCsvWriter.CompileSiteReportCsv(siteReports, request.StartDate, request.EndDate);

        return ApiResult<FileResponse>.Success(new FileResponse(csv.fileName, csv.fileContent));
    }

    protected override
        Task<(IReadOnlyCollection<ErrorMessageResponseItem> errors, SiteReportRequest request)>
        ReadRequestAsync(HttpRequest req)
    {
        var errors = new List<ErrorMessageResponseItem>();
        req.Query.TryGetValue("startDate", out var startDateInput);
        req.Query.TryGetValue("endDate", out var endDateInput);

        if (!DateOnly.TryParseExact(startDateInput.ToString(), "yyyy-MM-dd", out var startDate))
        {
            errors.Add(new ErrorMessageResponseItem()
            {
                Message = "Start Date is required",
                Property = "startDate"
            });
        }
        
        if (!DateOnly.TryParseExact(endDateInput.ToString(), "yyyy-MM-dd", out var endDate))
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
