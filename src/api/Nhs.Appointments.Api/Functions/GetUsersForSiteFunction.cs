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

public class GetUsersForSiteFunction(IUserService userService, IValidator<SiteBasedResourceRequest> validator, IUserContextProvider userContextProvider, ILogger<GetUsersForSiteFunction> logger)
    : SiteBasedResourceFunction<GetUsersForSiteResponse>(validator, userContextProvider, logger)
{

    [OpenApiOperation(operationId: "GetUsersForSite", tags: new[] { "User Management" }, Summary = "Get the users with roles for a given site")]
    [OpenApiRequestBody("text/json", typeof(SiteBasedResourceRequest))]
    [OpenApiParameter("site", Required = true, Description = "The id of the site to get users for")]
    [OpenApiSecurity("Api Key", SecuritySchemeType.ApiKey, Name = "Authorization", In = OpenApiSecurityLocationType.Header)]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, "text/plain", typeof(Site[]), Description = "List of users assigned to the site")]
    [Function("GetUsersForSiteFunction")]
    public override Task<IActionResult> RunAsync(
      [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "site/users")] HttpRequest req)
    {
        return base.RunAsync(req);
    }

    protected override async Task<ApiResult<GetUsersForSiteResponse>> HandleRequest(SiteBasedResourceRequest request, ILogger logger)
    {
        var permissionRequiredToViewAllUsers =
            (await userService.GetUserRoleAssignments(Principal.Claims.GetUserEmail()))
            .FirstOrDefault(roleAssignment => roleAssignment.Role == "users:view");
        if (permissionRequiredToViewAllUsers is null)
        {
            return ApiResult<GetUsersForSiteResponse>.Failed(HttpStatusCode.Forbidden, "Insufficient permissions.");
        }

        var users = (await userService.GetUsersForSite(request.Site)).Select(user =>
        {
            return new User()
            {
                Id = user.Id,
                RoleAssignments = user.RoleAssignments.Where(ra => ra.Scope == $"site:{request.Site}").ToArray()
            };
        });

        return ApiResult<GetUsersForSiteResponse>.Success(new GetUsersForSiteResponse(users));
    }

    protected override Task<IEnumerable<ErrorMessageResponseItem>> ValidateRequest(SiteBasedResourceRequest request)
    {
        return Task.FromResult(Enumerable.Empty<ErrorMessageResponseItem>());
    }
}

