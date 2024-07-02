using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Nhs.Appointments.Api.Models;
using Nhs.Appointments.Core;
using FluentValidation;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.OpenApi.Models;
using Nhs.Appointments.Api.Auth;
using System.Collections.Generic;
using System.Linq;

namespace Nhs.Appointments.Api.Functions;

public class GetSitesForUserFunction(ISiteSearchService siteSearchService, IUserService userService, IValidator<EmptyRequest> validator, IUserContextProvider userContextProvider, ILogger<GetSitesForUserFunction> logger)
    : BaseApiFunction<EmptyRequest, Site[]>(validator, userContextProvider, logger)
{

    [OpenApiOperation(operationId: "GetSitesForUser", tags: new[] { "Utility" }, Summary = "Set the status of a booking")]
    [OpenApiRequestBody("text/json", typeof(SetBookingStatusRequest))]
    [OpenApiSecurity("Api Key", SecuritySchemeType.ApiKey, Name = "Authorization", In = OpenApiSecurityLocationType.Header)]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, "text/plain", typeof(Site[]), Description = "List of sites available to the user")]    
    [Function("GetSitesForUserFunction")]
    public override Task<IActionResult> RunAsync(
      [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "user/sites")] HttpRequest req)
    {
        return base.RunAsync(req);
    }

    protected override async Task<ApiResult<Site[]>> HandleRequest(EmptyRequest request, ILogger logger)
    {
        var userEmail = Principal.Claims.GetUserEmail();
        var roleAssignments  = await userService.GetUserRoleAssignments(userEmail);
        var siteIdsForUser = roleAssignments.Where(ra => ra.Scope.StartsWith("site:")).Select(ra => ra.Scope.Replace("site:", ""));
        var siteInfoList = new List<Site>();

        foreach(var site in siteIdsForUser.Distinct()) 
        { 
            var siteInfo = await siteSearchService.GetSiteByIdAsync(site);
            siteInfoList.Add(siteInfo);            
        }
        
        return ApiResult<Site[]>.Success(siteInfoList.ToArray());
    }

    protected override Task<IEnumerable<ErrorMessageResponseItem>> ValidateRequest(EmptyRequest request)
    {
        return Task.FromResult(Enumerable.Empty<ErrorMessageResponseItem>());
    }
}

