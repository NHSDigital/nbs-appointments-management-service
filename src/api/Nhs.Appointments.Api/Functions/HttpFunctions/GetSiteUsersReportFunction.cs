using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Extensions.Logging;
using Nhs.Appointments.Api.Auth;
using Nhs.Appointments.Api.File;
using Nhs.Appointments.Api.Models;
using Nhs.Appointments.Core.Features;
using Nhs.Appointments.Core.Inspectors;
using Nhs.Appointments.Core.Reports.Users;
using Nhs.Appointments.Core.Sites;
using Nhs.Appointments.Core.Users;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace Nhs.Appointments.Api.Functions.HttpFunctions;

public class GetSiteUsersReportFunction(
    IUserService userService,
    IUserCsvWriter userCsvWriter,
    IFeatureToggleHelper featureToggleHelper,
    IValidator<GetSiteUsersReportRequest> validator,
    ILogger<GetSiteUsersReportFunction> logger,
    IMetricsRecorder metricsRecorder,
    IUserContextProvider userContextProvider) : BaseApiFunction<GetSiteUsersReportRequest, FileResponse>(validator, userContextProvider, logger, metricsRecorder)
{
    [OpenApiOperation(operationId: "GetSiteUsersReport", tags: ["SiteUsersReport"],
        Summary = "Get Daily Report")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, "text/csv",
        typeof(IEnumerable<SiteReport>),
        Description = "Report for all users for a given site")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, "application/json",
        typeof(ErrorMessageResponseItem), Description = "Unauthorized request to a protected API")]
    [Function("GetSiteUsersReportFunction")]
    [RequiresPermission(Permissions.SystemAdminUser, typeof(NoSiteRequestInspector))]
    public override async Task<IActionResult> RunAsync([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "report/site/{site}/users")]
        HttpRequest req)
    {
        return await featureToggleHelper.IsFeatureEnabled(Flags.ReportsUplift)
            ? await base.RunAsync(req)
            : ProblemResponse(HttpStatusCode.NotImplemented, "Reports uplift is not enabled.");
    }

    protected override async Task<ApiResult<FileResponse>> HandleRequest(GetSiteUsersReportRequest request, ILogger logger)
    {
        var users = await userService.GetUsersAsync(request.Site);
        var (fileName, fileContent) = await userCsvWriter.CompileSiteUsersReportCsv(request.Site, users);

        return ApiResult<FileResponse>.Success(new FileResponse(fileName, fileContent));
    }

    protected override Task<(IReadOnlyCollection<ErrorMessageResponseItem> errors, GetSiteUsersReportRequest request)> ReadRequestAsync(HttpRequest req)
    {
        var errors = new List<ErrorMessageResponseItem>();
        var site = req.HttpContext.GetRouteValue("site")?.ToString();

        if (string.IsNullOrEmpty(site))
        {
            errors.Add(new ErrorMessageResponseItem
            {
                Property = "site",
                Message = "Site is required."
            });
        }

        return Task.FromResult<(IReadOnlyCollection<ErrorMessageResponseItem> errors, GetSiteUsersReportRequest request)>
            ((errors, new GetSiteUsersReportRequest(site)));
    }

    protected override string ResponseType => ApiResponseType.File;
}
