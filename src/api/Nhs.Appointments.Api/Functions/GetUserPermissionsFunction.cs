using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Nhs.Appointments.Api.Auth;
using Nhs.Appointments.Api.Models;
using Nhs.Appointments.Core;

namespace Nhs.Appointments.Api.Functions;

public class GetUserPermissionsFunction(
    IPermissionChecker permissionChecker,
    IValidator<SiteBasedResourceRequest> validator,
    IUserContextProvider userContextProvider,
    ILogger<GetUserPermissionsFunction> logger,
    IMetricsRecorder metricsRecorder)
    : SiteBasedResourceFunction<PermissionsResponse>(validator, userContextProvider, logger, metricsRecorder)
{
    [OpenApiOperation(operationId: "GetPermissionsForAuthenticatedUser", tags: ["User"],
        Summary = "Gets all permissions for the authenticated user at a site")]
    [OpenApiParameter("site", In = ParameterLocation.Query, Required = true, Type = typeof(string),
        Description = "The id of the site to retrieve the authenticated user permissions for")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, "application/json", typeof(PermissionsResponse),
        Description = "List of permissions the authenticated user has at the requested site")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.BadRequest, "application/json",
        typeof(IEnumerable<ErrorMessageResponseItem>), Description = "The body of the request is invalid")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, "application/json",
        typeof(ErrorMessageResponseItem), Description = "Unauthorized request to a protected API")]
    [Function("GetPermissionsForUserFunction")]
    public override Task<IActionResult> RunAsync(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "user/permissions")]
        HttpRequest req)
    {
        return base.RunAsync(req);
    }

    protected override async Task<ApiResult<PermissionsResponse>> HandleRequest(SiteBasedResourceRequest request,
        ILogger logger)
    {
        var user = Principal.Claims.GetUserEmail();
        var permissions = await permissionChecker.GetPermissionsAsync(user, request.Site);
        var foo = 5;
        return ApiResult<PermissionsResponse>.Success(new PermissionsResponse { Permissions = permissions.ToArray() });
    } 
}
