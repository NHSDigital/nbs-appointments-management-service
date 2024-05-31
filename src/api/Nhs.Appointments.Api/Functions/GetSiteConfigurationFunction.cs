using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Nhs.Appointments.Api.Auth;
using Nhs.Appointments.Api.Models;
using Nhs.Appointments.Core;

namespace Nhs.Appointments.Api.Functions;

public class GetSiteConfigurationFunction  : SiteBasedResourceFunction<GetSiteConfigurationResponse>   
{
    private readonly IUserSiteAssignmentService _userSiteAssignmentService;
    private readonly ISiteConfigurationService _siteConfigurationService;
    private readonly ISiteSearchService _siteSearchService;
    
    public GetSiteConfigurationFunction(
        ISiteConfigurationService siteConfigurationService,
        ISiteSearchService siteSearchService,
        IUserSiteAssignmentService userSiteAssignmentService,
        IValidator<SiteBasedResourceRequest> validator,
        IUserContextProvider userContextProvider,
        ILogger<GetSiteConfigurationFunction> logger) : base(userSiteAssignmentService, validator, userContextProvider, logger)
    {
        _siteConfigurationService = siteConfigurationService;
        _siteSearchService = siteSearchService; 
        _userSiteAssignmentService = userSiteAssignmentService;
    }

    [OpenApiOperation(operationId: "GetSiteConfiguration", tags: new [] {"Site Configuration"}, Summary = "Get the site configuration")]
    [OpenApiRequestBody("text/json", typeof(SiteConfiguration))]
    [OpenApiParameter("site", Required = true, Description = "The id of the site to retrieve configuration for")]
    [OpenApiSecurity("Api Key", SecuritySchemeType.ApiKey, Name = "Authorization", In = OpenApiSecurityLocationType.Header)]
    [OpenApiResponseWithBody(statusCode:HttpStatusCode.OK, contentType: "text/json", typeof(SiteConfiguration),  Description = "The site configuration for the specified site")]
    [OpenApiResponseWithBody(statusCode:HttpStatusCode.BadRequest, contentType: "text/json", typeof(IEnumerable<ErrorMessageResponseItem>),  Description = "The body of the request is invalid")]
    [OpenApiResponseWithBody(statusCode:HttpStatusCode.NotFound, "text/json", typeof(ApiResult<object>), Description = "No site configuration was found for the specified site")]
    [Function("GetSiteConfiguration")]
    public override Task<IActionResult> RunAsync(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "site-configuration")] HttpRequest req)
    {
        return base.RunAsync(req);
    }
    
    protected override async Task<ApiResult<GetSiteConfigurationResponse>> HandleRequest(SiteBasedResourceRequest request, ILogger logger)
    {
        var siteId = await  GetSiteFromRequestAsync(request);
        if(string.IsNullOrEmpty(siteId)) 
        { 
            return request.ForUser ?
            Failed(System.Net.HttpStatusCode.Unauthorized, "") :
            Failed(System.Net.HttpStatusCode.BadRequest, "SiteId was not provided");
        }
        
        var siteDetails = await _siteSearchService.GetSiteByIdAsync(siteId);

        if (siteDetails != null)
        {
            var siteConfiguration = await _siteConfigurationService.GetSiteConfigurationOrDefaultAsync(siteId);
            var response = new GetSiteConfigurationResponse(siteDetails, siteConfiguration);
            return Success(response);
        }        
        
        return Failed(System.Net.HttpStatusCode.NotFound, "No site configuration was found for the specified site");
    }       
}