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
using Microsoft.OpenApi.Models;
using Nhs.Appointments.Api.Auth;
using Nhs.Appointments.Api.Models;
using Nhs.Appointments.Core;
using Nhs.Appointments.Core.Inspectors;

namespace Nhs.Appointments.Api.Functions
{
    public class GetUserRoleAssignmentsFunction(
        IUserService userService,
        IValidator<SiteBasedResourceRequest> validator,
        IUserContextProvider userContextProvider,
        ILogger<GetUserRoleAssignmentsFunction> logger,
        IMetricsRecorder metricsRecorder)
        : SiteBasedResourceFunction<IEnumerable<User>>(validator, userContextProvider, logger, metricsRecorder)
    {
        [OpenApiOperation(operationId: "Get_GetUserRoleAssignmentsFunction", tags: ["Users"],
            Summary = "Get all user roles assignments for a site")]
        [OpenApiParameter("site", In = ParameterLocation.Query, Required = true, Type = typeof(string),
            Description = "The id of the site to retrieve the user role assignments")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, "application/json", typeof(User[]),
            Description = "List of user role assignments for a site")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.BadRequest, "application/json",
            typeof(IEnumerable<ErrorMessageResponseItem>), Description = "The body of the request is invalid")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, "application/json",
            typeof(ErrorMessageResponseItem), Description = "Unauthorized request to a protected API")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Forbidden, "application/json",
            typeof(ErrorMessageResponseItem), Description = "Request failed due to insufficient permissions")]
        [RequiresPermission(Permissions.ViewUsers, typeof(SiteFromQueryStringInspector))]
        [Function("GetUserRoleAssignmentsFunction")]
        public override Task<IActionResult> RunAsync(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "users")] HttpRequest req)
        {
            return base.RunAsync(req);
        }

        protected override async Task<ApiResult<IEnumerable<User>>> HandleRequest(SiteBasedResourceRequest request,
            ILogger logger)
        {
            Func<RoleAssignment, bool> roleAssignmentFilter = ra => ra.Scope == $"site:{request.Site}";
            var users = await userService.GetUsersAsync(request.Site);
            var redactedUsers = users.Select(usr => new User
            {
                Id = usr.Id,
                RoleAssignments = usr.RoleAssignments.Where(roleAssignmentFilter).ToArray()
            });
            return ApiResult<IEnumerable<User>>.Success(redactedUsers);
        }
    }
}
