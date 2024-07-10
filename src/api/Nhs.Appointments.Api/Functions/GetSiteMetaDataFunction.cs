using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Nhs.Appointments.Api.Auth;
using Nhs.Appointments.Core;
using System.Threading.Tasks;
using Nhs.Appointments.Api.Models;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.OpenApi.Models;
using System.Collections.Generic;
using System.Net;
using Microsoft.Azure.Functions.Worker;

namespace Nhs.Appointments.Api.Functions;

public class GetSiteMetaDataFunction(ISiteConfigurationService siteConfigurationService, IValidator<SiteBasedResourceRequest> validator, IUserContextProvider userContextProvider, ILogger<GetSiteMetaDataFunction> logger)
    : SiteBasedResourceFunction<GetSiteMetaDataResponse>(validator, userContextProvider, logger)
{

    [OpenApiOperation(operationId: "GetSiteMetaData", tags: new[] { "Site Configuration" }, Summary = "Get meta data about the site specific to appointments")]
    [OpenApiRequestBody("text/json", typeof(SiteConfiguration))]
    [OpenApiParameter("site", Required = true, Description = "The id of the site to retrieve configuration for")]
    [OpenApiSecurity("Api Key", SecuritySchemeType.ApiKey, Name = "Authorization", In = OpenApiSecurityLocationType.Header)]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "text/json", typeof(GetSiteMetaDataResponse), Description = "The meta data for the specified site")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.BadRequest, contentType: "text/json", typeof(IEnumerable<ErrorMessageResponseItem>), Description = "The request did not contain a valid site in the query string")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.NotFound, "text/json", typeof(ApiResult<object>), Description = "No meta data was found for the specified site")]    
    [RequiresPermission("site:get-meta-data")]
    [Function("GetSiteMetaData")]
    public override Task<IActionResult> RunAsync(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "site/meta")] HttpRequest req)
    {
        return base.RunAsync(req);
    }

    protected override async Task<ApiResult<GetSiteMetaDataResponse>> HandleRequest(SiteBasedResourceRequest request, ILogger logger)
    {
        var siteConfiguration = await siteConfigurationService.GetSiteConfigurationOrDefaultAsync(request.Site);
        if (siteConfiguration != null)
        {
            return Success(new GetSiteMetaDataResponse(request.Site, siteConfiguration.InformationForCitizen));
        }

        return Failed(System.Net.HttpStatusCode.NotFound, "No site configuration was found for the specified site");
    }    
}
