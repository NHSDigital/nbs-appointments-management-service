using System.Collections.Generic;
using System.Linq;
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

public class SetUserRolesFunction(IUserService userService, IValidator<SetUserRolesRequest> validator, IUserContextProvider userContextProvider, ILogger<GetUserPermissionsFunction> logger) 
    : BaseApiFunction<SetUserRolesRequest, EmptyResponse>(validator, userContextProvider, logger)
{
    [OpenApiOperation(operationId: "SetUserRoles", tags: new[] { "Uer Roles" }, Summary = "Set user roles for a site")]
    [OpenApiRequestBody("text/json", typeof(SetUserRolesRequest))]
    [OpenApiSecurity("Api Key", SecuritySchemeType.ApiKey, Name = "Authorization", In = OpenApiSecurityLocationType.Header)]
    [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.OK, Description = "User role successfully saved")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.BadRequest, contentType: "text/json", typeof(IEnumerable<ErrorMessageResponseItem>), Description = "The body of the request is invalid")]
    [RequiresPermission("users:manage")]
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

        await userService.SaveUserAsync(request.User, request.Scope, roleAssignments);
        
        return Success(new EmptyResponse());
    }
    
}
