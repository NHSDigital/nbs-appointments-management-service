using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Extensions.Logging;
using Nhs.Appointments.Api.Auth;
using Nhs.Appointments.Api.Json;
using Nhs.Appointments.Api.Models;
using Nhs.Appointments.Core;
using Nhs.Appointments.Core.Features;
using Nhs.Appointments.Core.Inspectors;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace Nhs.Appointments.Api.Functions;

public class QuerySitesFunction(
    ISiteService siteService,
    IFeatureToggleHelper featureToggleHelper,
    IValidator<QuerySitesRequest> validator,
    IUserContextProvider userContextProvider,
    ILogger<QuerySitesFunction> logger,
    IMetricsRecorder metricsRecorder) : BaseApiFunction<QuerySitesRequest, IEnumerable<SiteWithDistance>>(validator, userContextProvider, logger, metricsRecorder)
{
    [OpenApiOperation(operationId: "QuerySites", tags: ["Sites"], Summary = "Query sites by a number of filters")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, "application/json", typeof(IEnumerable<SiteWithDistance>),
        Description = "A list of sites within the geographical area that support the provided filters.")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.BadRequest, "application/json",
        typeof(IEnumerable<ErrorMessageResponseItem>), Description = "The body of the request is invalid")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, "application/json",
        typeof(ErrorMessageResponseItem), Description = "Unauthorized request to a protected API")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.Forbidden, "application/json", typeof(ErrorMessageResponseItem),
        Description = "Request failed due to insufficient permissions")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.NotImplemented, "application/json",
        typeof(ErrorMessageResponseItem), Description = "Feature flag is not enabled.")]
    [RequiresPermission(Permissions.QuerySites, typeof(NoSiteRequestInspector))]
    [Function("QuerySitesFunction")]
    public override async Task<IActionResult> RunAsync(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "sites")]
        HttpRequest req)
    {
        return await featureToggleHelper.IsFeatureEnabled(Flags.QuerySites)
            ? await base.RunAsync(req)
            : ProblemResponse(HttpStatusCode.NotImplemented, null);
    }

    protected override async Task<ApiResult<IEnumerable<SiteWithDistance>>> HandleRequest(QuerySitesRequest request, ILogger logger)
    {
        var sites = await siteService.QuerySitesAsync(request.Filters, request.MaxRecords, request.IgnoreCache);
        return ApiResult<IEnumerable<SiteWithDistance>>.Success(sites);
    }

    protected override async Task<(IReadOnlyCollection<ErrorMessageResponseItem> errors, QuerySitesRequest request)> ReadRequestAsync(HttpRequest req)
    {
        var (errors, payload) = await JsonRequestReader.ReadRequestAsync<QuerySitesRequest>(req.Body);

        if (errors.Count > 0)
        {
            return (errors, null);
        }

        return (errors, payload);
    }
}
