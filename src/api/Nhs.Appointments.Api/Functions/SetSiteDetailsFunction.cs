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
using Nhs.Appointments.Core;
using Nhs.Appointments.Core.Inspectors;

namespace Nhs.Appointments.Api.Functions;

public class SetSiteDetailsFunction(
    ISiteService siteService,
    IValidator<SetSiteDetailsRequest> validator,
    IUserContextProvider userContextProvider,
    ILogger<SetSiteDetailsRequest> logger,
    IMetricsRecorder metricsRecorder)
    : BaseApiFunction<SetSiteDetailsRequest, EmptyResponse>(validator, userContextProvider, logger, metricsRecorder)
{
    [OpenApiOperation(operationId: "SetSiteDetails", tags: ["Sites"], Summary = "Set details for a site")]
    [OpenApiRequestBody("application/json", typeof(SetSiteDetailsRequest), Required = true)]
    [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.OK, Description = "Site details successfully saved")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.BadRequest, "application/json",
        typeof(IEnumerable<ErrorMessageResponseItem>), Description = "The body of the request is invalid")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.NotFound, "application/json", typeof(ApiResult<object>),
        Description = "Site details not found")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, "application/json",
        typeof(ErrorMessageResponseItem), Description = "Unauthorized request to a protected API")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.Forbidden, "application/json", typeof(ErrorMessageResponseItem),
        Description = "Request failed due to insufficient permissions")]
    [RequiresPermission("site:manage", typeof(SiteFromPathInspector))]
    [Function("SetSiteDetailsFunction")]
    public override Task<IActionResult> RunAsync(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "sites/{site}/details")]
        HttpRequest req)
    {
        return base.RunAsync(req);
    }

    protected override async Task<ApiResult<EmptyResponse>> HandleRequest(SetSiteDetailsRequest request, ILogger logger)
    {
        var result = await siteService.UpdateSiteDetailsAsync(request.Site, request.Name, request.Address, request.PhoneNumber, request.LatitudeDecimal, request.LongitudeDecimal);
        return result.Success ? Success(new EmptyResponse()) : Failed(HttpStatusCode.NotFound, result.Message);
    }

    protected override async Task<(IReadOnlyCollection<ErrorMessageResponseItem> errors, SetSiteDetailsRequest request)>
        ReadRequestAsync(HttpRequest req)
    {
        var site = req.HttpContext.GetRouteValue("site")?.ToString();
        var (errors, details) = await JsonRequestReader.ReadRequestAsync<DetailsRequest>(req.Body);
        return (errors,
            new SetSiteDetailsRequest(
                site,
                details.Name,
                details.Address,
                details.PhoneNumber,
                details.Latitude,
                details.Longitude));
    }
}
