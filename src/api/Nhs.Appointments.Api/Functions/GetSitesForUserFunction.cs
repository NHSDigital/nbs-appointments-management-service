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

public class GetSitesForUserFunction : BaseApiFunction<EmptyRequest, Site[]>
{
    private readonly ISiteSearchService _siteSearchService;
    private readonly IUserSiteAssignmentService _userSiteAssignmentService;

    public GetSitesForUserFunction(
        ISiteSearchService siteSearchService, 
        IUserSiteAssignmentService userSiteAssignmentService,
        IValidator<EmptyRequest> validator,
        IUserContextProvider userContextProvider,
        ILogger<GetSitesForUserFunction> logger) : base(validator, userContextProvider, logger)
    {
        _siteSearchService = siteSearchService;
        _userSiteAssignmentService = userSiteAssignmentService;
    }

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
        var userAssignments  = await _userSiteAssignmentService.GetUserAssignedSites(userEmail);

        var siteInfoList = new List<Site>();

        foreach(var assignment in userAssignments.Where(ua => ua.Site != "__global__")) 
        { 
            var siteInfo = await _siteSearchService.GetSiteByIdAsync(assignment.Site);
            siteInfoList.Add(siteInfo);            
        }

        return ApiResult<Site[]>.Success(siteInfoList.ToArray());
    }

    protected override Task<IEnumerable<ErrorMessageResponseItem>> ValidateRequest(EmptyRequest request)
    {
        return Task.FromResult(Enumerable.Empty<ErrorMessageResponseItem>());
    }
}

