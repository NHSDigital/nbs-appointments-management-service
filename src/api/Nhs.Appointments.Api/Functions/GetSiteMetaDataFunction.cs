using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Nhs.Appointments.Api.Auth;
using Nhs.Appointments.Core;
using System.Threading.Tasks;
using Nhs.Appointments.Api.Models;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using System.Collections.Generic;
using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.OpenApi.Models;

namespace Nhs.Appointments.Api.Functions;

public class GetSiteMetaDataFunction(ISiteService siteService, IValidator<SiteBasedResourceRequest> validator, IUserContextProvider userContextProvider, ILogger<GetSiteMetaDataFunction> logger)
    : SiteBasedResourceFunction<GetSiteMetaDataResponse>(validator, userContextProvider, logger)
{

    [OpenApiOperation(operationId: "GetSiteMetaData", tags: ["Sites"], Summary = "Get meta data about the site specific to appointments")]
    [OpenApiParameter("site", In = ParameterLocation.Query, Required = true, Type = typeof(string), Description = "The id of the site to retrieve configuration for")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, "application/json", typeof(GetSiteMetaDataResponse), Description = "The meta data for the specified site")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.BadRequest, "application/json", typeof(IEnumerable<ErrorMessageResponseItem>), Description = "The request did not contain a valid site in the query string")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.NotFound, "application/json", typeof(ApiResult<object>), Description = "No meta data was found for the specified site")]
    [OpenApiResponseWithBody(statusCode:HttpStatusCode.Unauthorized, "application/json", typeof(ErrorMessageResponseItem), Description = "Unauthorized request to a protected API")]
    [OpenApiResponseWithBody(statusCode:HttpStatusCode.Forbidden, "application/json", typeof(ErrorMessageResponseItem), Description = "Request failed due to insufficient permissions")]
    [RequiresPermission("site:get-meta-data", typeof(SiteFromQueryStringInspector))]
    [Function("GetSiteMetaData")]
    public override Task<IActionResult> RunAsync(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "sites/meta")] HttpRequest req)
    {
        return base.RunAsync(req);
    }

    protected override async Task<ApiResult<GetSiteMetaDataResponse>> HandleRequest(SiteBasedResourceRequest request, ILogger logger)
    {
        var site = await siteService.GetSiteByIdAsync(request.Site, request.Scope);
        if (site != null)
        {
            throw new System.NotImplementedException();
        }

        return Failed(HttpStatusCode.NotFound, "No site configuration was found for the specified site");
    }    
}
