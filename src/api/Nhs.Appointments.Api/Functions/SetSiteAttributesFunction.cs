using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Nhs.Appointments.Api.Auth;
using Nhs.Appointments.Api.Json;
using Nhs.Appointments.Api.Models;
using Nhs.Appointments.Core;

namespace Nhs.Appointments.Api.Functions;

public class SetSiteAttributesFunction(ISiteService siteService, IValidator<SetSiteAttributesRequest> validator, IUserContextProvider userContextProvider, ILogger<SetSiteAttributesFunction> logger, IMetricsRecorder metricsRecorder) 
    : BaseApiFunction<SetSiteAttributesRequest, EmptyResponse>(validator, userContextProvider, logger, metricsRecorder)
{
    [OpenApiOperation(operationId: "SetSiteAttributes", tags: ["Site Attributes"], Summary = "Set attribute values for a site")]
    [OpenApiRequestBody("text/json", typeof(SetSiteAttributesRequest))]
    [OpenApiSecurity("Api Key", SecuritySchemeType.ApiKey, Name = "Authorization", In = OpenApiSecurityLocationType.Header)]
    [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.OK, Description = "Site attribute values successfully saved")]
    [OpenApiResponseWithBody(statusCode:HttpStatusCode.Forbidden, "text/plain", typeof(ErrorMessageResponseItem), Description = "Request failed due to insufficient permissions")]
    [OpenApiResponseWithBody(statusCode:HttpStatusCode.Unauthorized, "text/plain", typeof(ErrorMessageResponseItem), Description = "Unauthorized request to a protected API")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.BadRequest, contentType: "text/json", typeof(IEnumerable<ErrorMessageResponseItem>), Description = "The body of the request is invalid")]
    [RequiresPermission("site:manage", typeof(SiteFromPathInspector))]
    [Function("SetSiteAttributesFunction")]
    public override Task<IActionResult> RunAsync(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "sites/{site}/attributes")] HttpRequest req)
    {
        return base.RunAsync(req);
    }

    protected override async Task<ApiResult<EmptyResponse>> HandleRequest(SetSiteAttributesRequest request, ILogger logger)
    {
        var result = await siteService.UpdateSiteAttributesAsync(request.Site, request.AttributeValues);
        return result.Success ? Success(new EmptyResponse()) : Failed(HttpStatusCode.NotFound, result.Message);
    }

    protected override async Task<(bool requestRead, SetSiteAttributesRequest request)> ReadRequestAsync(HttpRequest req)
    {
        var site = req.HttpContext.GetRouteValue("site")?.ToString();
        var attributes = await JsonRequestReader.ReadRequestAsync<IEnumerable<AttributeValue>>(req.Body);
        return (true, new SetSiteAttributesRequest(site, attributes));
    }
}
