using System.Collections.Generic;
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
using Microsoft.OpenApi.Models;
using Nhs.Appointments.Api.Auth;

namespace Nhs.Appointments.Api.Functions;

public class GetUserPermissionsFunction(IPermissionChecker permissionChecker, IValidator<SiteBasedResourceRequest> validator, IUserContextProvider userContextProvider, ILogger<GetUserPermissionsFunction> logger)
    : SiteBasedResourceFunction<PermissionsResponse>(validator, userContextProvider, logger)
{

    [OpenApiOperation(operationId: "GetPermissionsForAuthenticatedUser", tags: ["User"], Summary = "Gets all permissions for the authenticated user at a site")]
    [OpenApiParameter("site", In = ParameterLocation.Query, Required = true, Type = typeof(string), Description = "The id of the site to retrieve the authenticated user permissions for")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, "application/json", typeof(PermissionsResponse), Description = "List of permissions the authenticated user has at the requested site")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.BadRequest, "application/json", typeof(IEnumerable<ErrorMessageResponseItem>), Description = "The body of the request is invalid")]
    [OpenApiResponseWithBody(statusCode:HttpStatusCode.Unauthorized, "application/json", typeof(ErrorMessageResponseItem), Description = "Unauthorized request to a protected API")]
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