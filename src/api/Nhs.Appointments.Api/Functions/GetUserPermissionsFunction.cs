using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Nhs.Appointments.Api.Models;
using FluentValidation;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.OpenApi.Models;
using Nhs.Appointments.Api.Auth;

namespace Nhs.Appointments.Api.Functions;

public class GetUserPermissionsFunction(IPermissionChecker permissionChecker, IValidator<SiteBasedResourceRequest> validator, IUserContextProvider userContextProvider, ILogger<GetUserPermissionsFunction> logger, IMetricsRecorder metricsRecorder)
    : SiteBasedResourceFunction<PermissionsResponse>(validator, userContextProvider, logger, metricsRecorder)
{

    [OpenApiOperation(operationId: "GetPermissionsForUser", tags: new[] { "Auth" }, Summary = "Gets the users for a given user and site")]
    [OpenApiSecurity("Api Key", SecuritySchemeType.ApiKey, Name = "Authorization", In = OpenApiSecurityLocationType.Header)]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, "text/plain", typeof(PermissionsResponse), Description = "List of permissions the user has at the specified site")]    
    [Function("GetPermissionsForUserFunction")]
    public override Task<IActionResult> RunAsync(
      [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "user/permissions")] HttpRequest req)
    {
        return base.RunAsync(req);
    }

    protected override async Task<ApiResult<PermissionsResponse>> HandleRequest(SiteBasedResourceRequest request, ILogger logger)
    {
        var user = Principal.Claims.GetUserEmail();
        var permissions = await permissionChecker.GetPermissionsAsync(user, request.Site);
        return ApiResult<PermissionsResponse>.Success(new PermissionsResponse{ Permissions = permissions.ToArray()});
    }
}