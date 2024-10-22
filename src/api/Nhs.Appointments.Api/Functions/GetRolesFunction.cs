﻿using System.Linq;
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

namespace Nhs.Appointments.Api.Functions;

public class GetRolesFunction(IRolesService rolesService, IValidator<GetRolesRequest> validator, IUserContextProvider userContextProvider, ILogger<GetRolesFunction> logger ) 
    : BaseApiFunction<GetRolesRequest, GetRolesResponse>(validator, userContextProvider, logger)
{
    [OpenApiOperation(operationId: "GetRoles", tags: ["Roles"], Summary = "Get user roles in the system")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, "text/plain", typeof(GetRolesResponse), Description = "List of roles with id and display name information")]  
    [OpenApiResponseWithBody(statusCode:HttpStatusCode.Unauthorized, "text/plain", typeof(ErrorMessageResponseItem), Description = "Unauthorized request to a protected API")]
    [Function("GetRolesFunction")]
    public override Task<IActionResult> RunAsync(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "roles")] HttpRequest req)
    {
        return base.RunAsync(req);
    }

    protected override async Task<ApiResult<GetRolesResponse>> HandleRequest(GetRolesRequest request, ILogger logger)
    {
        var roles = await rolesService.GetRoles();
        var mappedRoles = roles
            .Where(r => r.Id.StartsWith(request.tag))
            .Select(role => new GetRoleResponseItem(role.Name, role.Id, role.Description));
        return ApiResult<GetRolesResponse>.Success(new GetRolesResponse(mappedRoles.ToArray()));
    }
    
    protected override Task<(bool requestRead, GetRolesRequest request)> ReadRequestAsync(HttpRequest req)
    {
        var tag = req.Query["tag"];
        return Task.FromResult<(bool requestRead, GetRolesRequest request)>((true, new GetRolesRequest(tag)));
    }
}
