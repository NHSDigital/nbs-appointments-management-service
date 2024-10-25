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

public class GetRolesFunction(IRolesService rolesService, IValidator<GetRolesRequest> validator, IUserContextProvider userContextProvider, ILogger<GetRolesFunction> logger, IMetricsRecorder metricsRecorder) 
    : BaseApiFunction<GetRolesRequest, GetRolesResponse>(validator, userContextProvider, logger, metricsRecorder)
{
    [OpenApiOperation(operationId: "GetRoles", tags: new[] { "Auth" }, Summary = "Gets all existing roles")]
    [OpenApiSecurity("Api Key", SecuritySchemeType.ApiKey, Name = "Authorization", In = OpenApiSecurityLocationType.Header)]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, "text/plain", typeof(GetRolesResponse), Description = "List of roles with name and id information")]    
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
