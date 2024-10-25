using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Extensions.Logging;
using Nhs.Appointments.Api.Auth;
using Nhs.Appointments.Api.Models;
using Nhs.Appointments.Core;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Nhs.Appointments.Api.Functions
{
    public class GetUserProfileFunction(ISiteService siteService, IUserService userService, IValidator<EmptyRequest> validator, IUserContextProvider userContextProvider, ILogger<GetUserProfileFunction> logger)
    : BaseApiFunction<EmptyRequest, UserProfile>(validator, userContextProvider, logger)
    {

        [OpenApiOperation(operationId: "GetUserProfile", tags: ["User"], Summary = "Gets information about the signed in user")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, "application/json", typeof(UserProfile), Description = "Information about the signed in user")]
        [Function("GetUserProfileFunction")]
        public override Task<IActionResult> RunAsync(
          [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "user/profile")] HttpRequest req)
        {
            return base.RunAsync(req);
        }

        protected override async Task<ApiResult<UserProfile>> HandleRequest(EmptyRequest request, ILogger logger)
        {
            var userEmail = Principal.Claims.GetUserEmail();
            var roleAssignments = await userService.GetUserRoleAssignments(userEmail);
            var siteIdsForUser = roleAssignments.Where(ra => ra.Scope.StartsWith("site:")).Select(ra => ra.Scope.Replace("site:", ""));
            var siteInfoList = new List<UserProfileSite>();

            foreach (var site in siteIdsForUser.Distinct())
            {
                var siteInfo = await siteService.GetSiteByIdAsync(site);
                if(siteInfo != null)
                    siteInfoList.Add(new UserProfileSite(siteInfo.Id, siteInfo.Name, siteInfo.Address));
            }

            return ApiResult<UserProfile>.Success(new UserProfile(userEmail, siteInfoList));
        }

        protected override Task<IEnumerable<ErrorMessageResponseItem>> ValidateRequest(EmptyRequest request)
        {
            return Task.FromResult(Enumerable.Empty<ErrorMessageResponseItem>());
        }
    }
}
