using System;
using System.Collections.Generic;
using System.IO;
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

namespace Nhs.Appointments.Api.Functions;

public class GetReportSiteSummaryFunction(
    IUserService userService,
    IPermissionChecker permissionChecker,
    ISiteReportService siteReportService,
    TimeProvider timeProvider,
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
            ? ProblemResponse(HttpStatusCode.NotImplemented, null)
            : await base.RunAsync(req);
    }

    protected override string ResponseType => ApiResponseType.File;

    protected override async Task<ApiResult<FileResponse>> HandleRequest(SiteReportRequest request,
        ILogger logger)
    {
        var userEmail = Principal.Claims.GetUserEmail();

        var user = await userService.GetUserAsync(userEmail);
        var fileName =
            $"SiteReport_{request.StartDate:yyyy-MM-dd}_{request.EndDate:yyyy-MM-dd}_{timeProvider.GetUtcNow():yyyyMMddhhmmss}.csv";

        var sites = await permissionChecker.GetSitesWithPermissionAsync(user.Id, Permissions.ReportsSiteSummary);
        
        var report = await siteReportService.Generate(sites.ToArray(), request.StartDate, request.EndDate);
        
        return ApiResult<FileResponse>.Success(new FileResponse(fileName, await ReportToCsv(report.ToArray())));
    }

    private async Task<MemoryStream> ReportToCsv(SiteReport[] rows)
    {
        var memoryStream = new MemoryStream();
        await using var streamWriter = new StreamWriter(memoryStream);
        await ProcessToFile(streamWriter, rows);
        return memoryStream;
    }
    
    private async Task ProcessToFile(TextWriter csvWriter, SiteReport[] rows)
    {
        var distinctServices = rows.SelectMany(x => x.Bookings.Keys).Union(rows.SelectMany(x => x.RemainingCapacity.Keys))
            .Distinct().ToArray();
        var distinctCancellationReasons = rows.SelectMany(x => x.Cancelled.Keys).ToArray();
        
        await csvWriter.WriteLineAsync(string.Join(',', SiteReportMap.Headers(distinctServices, distinctCancellationReasons)));
        foreach (var row in rows)
        {
            await csvWriter.WriteLineAsync(string.Join(',', [
                SiteReportMap.SiteName(row),
                SiteReportMap.ICB(row),
                SiteReportMap.Region(row),
                SiteReportMap.OdsCode(row),
                SiteReportMap.Longitude(row),
                SiteReportMap.Latitude(row),
                string.Join(',', distinctServices.Select(service => SiteReportMap.BookingsCount(row, service))),
                string.Join(',', distinctCancellationReasons.Select(reason => SiteReportMap.BookingsCount(row, reason))),
                SiteReportMap.TotalBookings(row).ToString(),
                SiteReportMap.MaximumCapacity(row).ToString(),
                string.Join(',', distinctServices.Select(service => SiteReportMap.CapacityCount(row, service)))
            ]));
        }
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

public static class SiteReportMap
{
    public static string[] Headers(string[] services, string[] cancelationReasons)
    {
        var siteHeaders = new[] { "Site Name", "ICB", "Region", "ODS Code", "Longitude", "Latitude" };
        var statHeaders = new[] { "Total Bookings", "Maximum Capacity" };
        var bookingsHeaders = services.Select(service => $"{service} Booked");
        var capacityHeaders = services.Select(service => $"{service} Capacity");
            
        return siteHeaders
            .Union(bookingsHeaders)
            .Union(cancelationReasons)
            .Union(statHeaders)
            .Union(capacityHeaders)
            .ToArray();
    } 
    public static string SiteName(SiteReport report) => report.SiteName;
    public static string ICB(SiteReport report) => report.ICB;
    public static string Region(SiteReport report) => report.Region;
    public static string OdsCode(SiteReport report) => report.OdsCode;
    public static double Longitude(SiteReport report) => report.Longitude;
    public static double Latitude(SiteReport report) => report.Latitude;
    public static int TotalBookings(SiteReport report) => report.TotalBookings;
    public static int Cancelled(SiteReport report, string key) => report.Cancelled.GetValueOrDefault(key, 0);
    public static int MaximumCapacity(SiteReport report) => report.MaximumCapacity;
    public static int BookingsCount(SiteReport report, string key) => report.Bookings.GetValueOrDefault(key, 0);
    public static int CapacityCount(SiteReport report, string key) => report.RemainingCapacity.GetValueOrDefault(key, 0);
}
