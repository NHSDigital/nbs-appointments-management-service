using System;
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
using Nhs.Appointments.Api.Auth;
using Nhs.Appointments.Api.Models;
using Nhs.Appointments.Core;
using Nhs.Appointments.Core.Inspectors;

namespace Nhs.Appointments.Api.Functions;

public class SetUserRolesFunction(
    IUserService userService,
    IValidator<SetUserRolesRequest> validator,
    IUserContextProvider userContextProvider,
    ILogger<SetUserRolesFunction> logger,
    IMetricsRecorder metricsRecorder)
    : BaseApiFunction<SetUserRolesRequest, EmptyResponse>(validator, userContextProvider, logger, metricsRecorder)
{
    [OpenApiOperation(operationId: "SetUserRoles", tags: ["User"],
        Summary = "Set role assignments for a user at a site")]
    [OpenApiRequestBody("application/json", typeof(SetUserRolesRequest), Required = true)]
    [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.OK, Description = "User role successfully saved")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.BadRequest, "application/json",
        typeof(IEnumerable<ErrorMessageResponseItem>), Description = "The body of the request is invalid")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, "application/json",
        typeof(ErrorMessageResponseItem), Description = "Unauthorized request to a protected API")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.Forbidden, "application/json", typeof(ErrorMessageResponseItem),
        Description = "Request failed due to insufficient permissions")]
    [RequiresPermission(Permissions.ManageUsers, typeof(SiteFromScopeInspector))]
    [Function("SetUserRoles")]
    public override Task<IActionResult> RunAsync(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "user/roles")]
        HttpRequest req)
    {
        return base.RunAsync(req);
    }

    protected override async Task<ApiResult<EmptyResponse>> HandleRequest(SetUserRolesRequest request, ILogger logger)
    {
        if (userContextProvider.UserPrincipal.Claims.GetUserEmail() == request.User)
        {
            return Failed(HttpStatusCode.BadRequest,
                "You cannot update the role assignments of the currently logged in user.");
        }

        var roleAssignments = request
            .Roles
            .Select(
                role => new RoleAssignment { Role = role, Scope = request.Scope })
            .ToList();

        var result = await userService.UpdateUserRoleAssignmentsAsync(request.User, request.Scope, roleAssignments);

        if (!result.Success)
        {
            return Failed(HttpStatusCode.BadRequest, FormatError(result));
        }

        return Success(new EmptyResponse());
    }

    private static string FormatError(UpdateUserRoleAssignmentsResult result)
    {
        if (result.Success)
        {
            throw new Exception("The User Service operation succeeded so there is no error message to generate.");
        }

        if (string.IsNullOrEmpty(result.ErrorUser) && !result.ErrorRoles.Any())
        {
            throw new Exception("The User Service returned an error but did not provide further information");
        }

        return $"Invalid role(s): {string.Join(", ", result.ErrorRoles)}";
    }
}
