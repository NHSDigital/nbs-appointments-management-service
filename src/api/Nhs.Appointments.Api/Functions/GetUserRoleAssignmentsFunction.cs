using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Nhs.Appointments.Api.Auth;
using Nhs.Appointments.Api.Models;
using Nhs.Appointments.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nhs.Appointments.Api.Functions
{
    public class GetUserRoleAssignmentsFunction(IUserService userService, IValidator<SiteBasedResourceRequest> validator, IUserContextProvider userContextProvider, ILogger<GetUserRoleAssignmentsFunction> logger)
        : SiteBasedResourceFunction<IEnumerable<User>>(validator, userContextProvider, logger)
    {                
        [Function("GetUserRoleAssignmentsFunction")]
        public override Task<IActionResult> RunAsync([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "users")] HttpRequest req)
        {
            return base.RunAsync(req);
        }

        protected override async Task<ApiResult<IEnumerable<User>>> HandleRequest(SiteBasedResourceRequest request, ILogger logger)
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
