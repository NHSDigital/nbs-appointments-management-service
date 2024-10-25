using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
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

public class SetUserRolesFunction(IUserService userService, IValidator<SetUserRolesRequest> validator, IUserContextProvider userContextProvider, ILogger<SetUserRolesFunction> logger, IMetricsRecorder metricsRecorder) 
    : BaseApiFunction<SetUserRolesRequest, EmptyResponse>(validator, userContextProvider, logger, metricsRecorder)
{
    [OpenApiOperation(operationId: "SetUserRoles", tags: new[] { "User Roles" }, Summary = "Set user roles for a site")]
    [OpenApiRequestBody("text/json", typeof(SetUserRolesRequest))]
    [OpenApiSecurity("Api Key", SecuritySchemeType.ApiKey, Name = "Authorization", In = OpenApiSecurityLocationType.Header)]
    [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.OK, Description = "User role successfully saved")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.BadRequest, contentType: "text/json", typeof(IEnumerable<ErrorMessageResponseItem>), Description = "The body of the request is invalid")]
    [RequiresPermission("users:manage", typeof(SiteFromScopeInspector))]
    [Function("SetUserRoles")]
    public override Task<IActionResult> RunAsync(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "user/roles")] HttpRequest req)
    {
        return base.RunAsync(req);
    }

    protected override async Task<ApiResult<EmptyResponse>> HandleRequest(SetUserRolesRequest request, ILogger logger)
    {
        var roleAssignments = request
            .Roles
            .Select(
                role => new RoleAssignment()
                {
                    Role = role,
                    Scope = request.Scope
                })
            .ToList();

        var result = await userService.UpdateUserRoleAssignmentsAsync(request.User, request.Scope, roleAssignments);

        if(!result.Success)
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

        if(string.IsNullOrEmpty(result.ErrorUser) && !result.ErrorRoles.Any())
        {
            throw new Exception("The User Service returned an error but did not provide further information");
        }
        
        return $"Invalid role(s): {string.Join(", ", result.ErrorRoles)}";
    }
}


