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

public class SetSiteAccessibilitiesFunction(
    ISiteService siteService,
    IValidator<SetSiteAccessibilitiesRequest> validator,
    IUserContextProvider userContextProvider,
    ILogger<SetSiteAccessibilitiesFunction> logger,
    IMetricsRecorder metricsRecorder)
    : BaseApiFunction<SetSiteAccessibilitiesRequest, EmptyResponse>(validator, userContextProvider, logger,
        metricsRecorder)
{
    [OpenApiOperation(operationId: "SetSiteAccessibilities", tags: ["Sites"],
        Summary = "Set accessibilities values for a site")]
    [OpenApiRequestBody("application/json", typeof(SetSiteAccessibilitiesRequest), Required = true)]
    [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.OK,
        Description = "Site accessibility values successfully saved")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.BadRequest, "application/json",
        typeof(IEnumerable<ErrorMessageResponseItem>), Description = "The body of the request is invalid")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.NotFound, "application/json", typeof(ApiResult<object>),
        Description = "Booking not found")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, "application/json",
        typeof(ErrorMessageResponseItem), Description = "Unauthorized request to a protected API")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.Forbidden, "application/json", typeof(ErrorMessageResponseItem),
        Description = "Request failed due to insufficient permissions")]
    [RequiresPermission(Permissions.ManageSite, typeof(SiteFromPathInspector))]
    [RequiresAudit(typeof(SiteFromPathInspector))]
    [Function("SetSiteAccessibilitiesFunction")]
    public override Task<IActionResult> RunAsync(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "sites/{site}/accessibilities")]
        HttpRequest req)
    {
        return base.RunAsync(req);
    }

    protected override async Task<ApiResult<EmptyResponse>> HandleRequest(SetSiteAccessibilitiesRequest request,
        ILogger logger)
    {
        var result = await siteService.UpdateAccessibilities(request.Site, request.Accessibilities);
        return result.Success ? Success(new EmptyResponse()) : Failed(HttpStatusCode.NotFound, result.Message);
    }

    protected override async
        Task<(IReadOnlyCollection<ErrorMessageResponseItem> errors, SetSiteAccessibilitiesRequest request)>
        ReadRequestAsync(HttpRequest req)
    {
        var site = req.HttpContext.GetRouteValue("site")?.ToString();
        var (errors, request) = await JsonRequestReader.ReadRequestAsync<AccessibilityRequest>(req.Body);
        return (errors, new SetSiteAccessibilitiesRequest(site, request?.Accessibilities));
    }
}
