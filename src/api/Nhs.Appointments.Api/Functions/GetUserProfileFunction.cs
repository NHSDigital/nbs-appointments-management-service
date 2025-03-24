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
using Nhs.Appointments.Api.Models;
using Nhs.Appointments.Core;

namespace Nhs.Appointments.Api.Functions
{
    public class GetUserProfileFunction(
        IUserService userService,
        IValidator<EmptyRequest> validator,
        IUserContextProvider userContextProvider,
        ILogger<GetUserProfileFunction> logger,
        IMetricsRecorder metricsRecorder
    ) : BaseApiFunction<EmptyRequest, UserProfile>(validator, userContextProvider, logger, metricsRecorder)
    {
        [OpenApiOperation(operationId: "GetUserProfile", tags: ["User"],
            Summary = "Gets information about the signed in user")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, "application/json", typeof(UserProfile),
            Description = "Information about the signed in user")]
        [Function("GetUserProfileFunction")]
        public override Task<IActionResult> RunAsync(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "user/profile")]
            HttpRequest req)
        {
            return base.RunAsync(req);
        }

        protected override async Task<ApiResult<UserProfile>> HandleRequest(EmptyRequest request, ILogger logger)
        {
            var userEmail = Principal.Claims.GetUserEmail();

            var user = await userService.GetUserAsync(userEmail);
            if (user is null)
            {
                return Success(new UserProfile(userEmail, false, null));
            }

            var hasSites = user.RoleAssignments
                .Where(ra => ra.Scope.StartsWith("site:") || ra.Scope.StartsWith("global"))
                .Count() > 0;

            return ApiResult<UserProfile>.Success(new UserProfile(userEmail, hasSites, user.LatestAcceptedEulaVersion));
        }

        protected override Task<IEnumerable<ErrorMessageResponseItem>> ValidateRequest(EmptyRequest request)
        {
            return Task.FromResult(Enumerable.Empty<ErrorMessageResponseItem>());
        }
    }
}
