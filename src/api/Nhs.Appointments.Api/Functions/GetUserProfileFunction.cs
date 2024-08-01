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
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Nhs.Appointments.Api.Functions
{
    public class GetUserProfileFunction(IValidator<EmptyRequest> validator, IUserContextProvider userContextProvider, ILogger<GetSitesForUserFunction> logger)
    : BaseApiFunction<EmptyRequest, UserProfile>(validator, userContextProvider, logger)
    {

        [OpenApiOperation(operationId: "GetUserProfile", tags: new[] { "Utility" }, Summary = "Gets information about the signed in user")]
        [OpenApiRequestBody("text/json", typeof(SetBookingStatusRequest))]
        [OpenApiSecurity("Api Key", SecuritySchemeType.ApiKey, Name = "Authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, "text/plain", typeof(UserProfile), Description = "Information about the signed in user")]
        [Function("GetUserProfileFunction")]
        public override Task<IActionResult> RunAsync(
          [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "user/profile")] HttpRequest req)
        {
            return base.RunAsync(req);
        }

        protected override Task<ApiResult<UserProfile>> HandleRequest(EmptyRequest request, ILogger logger)
        {
            var userEmail = Principal.Claims.GetUserEmail();
            return Task.FromResult(ApiResult<UserProfile>.Success(new UserProfile(userEmail)));
        }

        protected override Task<IEnumerable<ErrorMessageResponseItem>> ValidateRequest(EmptyRequest request)
        {
            return Task.FromResult(Enumerable.Empty<ErrorMessageResponseItem>());
        }
    }
}
