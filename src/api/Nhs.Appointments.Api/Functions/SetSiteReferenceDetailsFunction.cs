using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Extensions.Logging;
using Nhs.Appointments.Api.Auth;
using Nhs.Appointments.Api.Json;
using Nhs.Appointments.Api.Models;
using Nhs.Appointments.Audit.Functions;
using Nhs.Appointments.Core;
using Nhs.Appointments.Core.Inspectors;

namespace Nhs.Appointments.Api.Functions;

public class SetSiteReferenceDetailsFunction(
    ISiteService siteService,
    IValidator<SetSiteReferenceDetailsRequest> validator,
    IUserContextProvider userContextProvider,
    ILogger<SetSiteReferenceDetailsRequest> logger,
    IMetricsRecorder metricsRecorder)
    : BaseApiFunction<SetSiteReferenceDetailsRequest, EmptyResponse>(validator, userContextProvider, logger, metricsRecorder)
{
    [OpenApiOperation(operationId: "SetSiteReferenceDetails", tags: ["Sites"], Summary = "Set reference details for a site")]
    [OpenApiRequestBody("application/json", typeof(SetSiteDetailsRequest), Required = true)]
    [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.OK, Description = "Site reference details successfully saved")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.BadRequest, "application/json",
        typeof(IEnumerable<ErrorMessageResponseItem>), Description = "The body of the request is invalid")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.NotFound, "application/json", typeof(ApiResult<object>),
        Description = "Site reference details not found")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, "application/json",
        typeof(ErrorMessageResponseItem), Description = "Unauthorized request to a protected API")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.Forbidden, "application/json", typeof(ErrorMessageResponseItem),
        Description = "Request failed due to insufficient permissions")]
    [RequiresPermission(Permissions.SystemAdmin, typeof(SiteFromPathInspector))]
    [RequiresAudit(typeof(SiteFromPathInspector))]
    [Function("SetSiteReferenceDetailsFunction")]
    public override Task<IActionResult> RunAsync(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "sites/{site}/reference-details")]
        HttpRequest req)
    {
        return base.RunAsync(req);
    }

    protected override async Task<ApiResult<EmptyResponse>> HandleRequest(SetSiteReferenceDetailsRequest request, ILogger logger)
    {
        var result = await siteService.UpdateSiteReferenceDetailsAsync(request.Site, request.OdsCode, request.Icb, request.Region);
        return result.Success ? Success(new EmptyResponse()) : Failed(HttpStatusCode.NotFound, result.Message);
    }

    protected override async Task<(IReadOnlyCollection<ErrorMessageResponseItem> errors, SetSiteReferenceDetailsRequest request)>
        ReadRequestAsync(HttpRequest req)
    {
        var site = req.HttpContext.GetRouteValue("site")?.ToString();
        var (errors, details) = await JsonRequestReader.ReadRequestAsync<ReferenceDetailsRequest>(req.Body);
        return (errors,
            new SetSiteReferenceDetailsRequest(
                site,
                details.OdsCode,
                details.Icb,
                details.Region));
    }
}
