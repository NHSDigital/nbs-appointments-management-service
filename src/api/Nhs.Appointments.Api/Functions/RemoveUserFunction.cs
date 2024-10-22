using System.Collections.Generic;
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
using Nhs.Appointments.Api.Auth;

namespace Nhs.Appointments.Api.Functions;

public class RemoveUserFunction(
    IUserService userService,
    IValidator<RemoveUserRequest> validator,
    IUserContextProvider userContextProvider,
    ILogger<RemoveUserFunction> logger)
    : BaseApiFunction<RemoveUserRequest, RemoveUserResponse>(validator, userContextProvider, logger)
{
    [OpenApiOperation(operationId: "RemoveUser", tags: ["User"], Summary = "Remove a User")]
    [OpenApiRequestBody("text/json", typeof(RemoveUserRequest))]
    [OpenApiResponseWithBody(statusCode:HttpStatusCode.OK, "text/json", typeof(RemoveUserRequest), Description = "User successfully removed")]
    [OpenApiResponseWithBody(statusCode:HttpStatusCode.BadRequest, contentType: "text/json", typeof(IEnumerable<ErrorMessageResponseItem>),  Description = "The body of the request is invalid" )]
    [OpenApiResponseWithBody(statusCode:HttpStatusCode.NotFound, "text/plain", typeof(string), Description = "User did not exist to be removed")]
    [RequiresPermission("users:manage", typeof(SiteFromBodyInspector))]
    [Function("RemoveUserFunction")]
    public override Task<IActionResult> RunAsync(
       [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "user/remove")] HttpRequest req)
    {
        return base.RunAsync(req);
    }

    protected override async Task<ApiResult<RemoveUserResponse>> HandleRequest(RemoveUserRequest request, ILogger logger)
    {
        if (userContextProvider.UserPrincipal.Claims.GetUserEmail() == request.User)
        {
            return Failed(HttpStatusCode.BadRequest, "You cannot remove the currently logged in user.");
        }

        var result = await userService.RemoveUserAsync(request.User, request.Site);
        return result.Success ? Success(new RemoveUserResponse(request.User, request.Site)) : Failed(HttpStatusCode.NotFound, result.Message);
    }
}