using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Extensions.Logging;
using Nhs.Appointments.Api.Auth;
using Nhs.Appointments.Api.File;
using Nhs.Appointments.Api.Models;
using Nhs.Appointments.Core.Features;
using Nhs.Appointments.Core.Inspectors;
using Nhs.Appointments.Core.Reports.MasterSiteList;
using Nhs.Appointments.Core.Sites;
using Nhs.Appointments.Core.Users;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace Nhs.Appointments.Api.Functions.HttpFunctions;

public class GetReportMasterSiteListFunction(
    ISiteService siteService,
    IMasterSiteListReportCsvWriter masterSiteListReportCsvWriter,
    IFeatureToggleHelper featureToggleHelper,
    IValidator<EmptyRequest> validator,
    IUserContextProvider userContextProvider,
    ILogger<GetReportMasterSiteListFunction> logger,
    IMetricsRecorder metricsRecorder)
    : BaseApiFunction<EmptyRequest, FileResponse>(validator, userContextProvider, logger,
        metricsRecorder)
{
    [OpenApiOperation(operationId: "GetMasterSiteListReport", tags: ["MasterSiteListReport"],
        Summary = "Get Master Site List Report")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, "text/csv",
        typeof(IEnumerable<Site>),
        Description = "Report for all Sites")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, "application/json",
        typeof(ErrorMessageResponseItem), Description = "Unauthorized request to a protected API")]
    [RequiresPermission(Permissions.ReportsMasterSiteList, typeof(AnyUserSitesRequestInspector))]
    [Function("GetReportMasterSiteListFunction")]
    public override async Task<IActionResult> RunAsync(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "report/master-site-list")]
        HttpRequest req)
    {
        return !await featureToggleHelper.IsFeatureEnabled(Flags.ReportsUplift)
            ? ProblemResponse(HttpStatusCode.NotImplemented, "Master Site List Reports are not enabled.")
            : await base.RunAsync(req);
    }

    protected override string ResponseType => ApiResponseType.File;

    protected override async Task<ApiResult<FileResponse>> HandleRequest(EmptyRequest request,
        ILogger logger)
    {
        var sites = await siteService.GetAllSites(includeDeleted: true, ignoreCache: true);

        var csv = await masterSiteListReportCsvWriter.CompileMasterSiteListReportCsv(sites);

        return ApiResult<FileResponse>.Success(new FileResponse(csv.fileName, csv.fileContent));
    }

    protected override Task<(IReadOnlyCollection<ErrorMessageResponseItem> errors, EmptyRequest request)>
        ReadRequestAsync(HttpRequest req)
    {
        return Task.FromResult<(IReadOnlyCollection<ErrorMessageResponseItem>, EmptyRequest)>(
            (Array.Empty<ErrorMessageResponseItem>(), new EmptyRequest())
        );
    }
}
