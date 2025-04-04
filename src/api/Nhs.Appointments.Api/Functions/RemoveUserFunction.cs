﻿using System.Collections.Generic;
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
using Nhs.Appointments.Core.Inspectors;

namespace Nhs.Appointments.Api.Functions;

public class RemoveUserFunction(
    IUserService userService,
    IValidator<RemoveUserRequest> validator,
    IUserContextProvider userContextProvider,
    ILogger<RemoveUserFunction> logger,
    IMetricsRecorder metricsRecorder)
    : BaseApiFunction<RemoveUserRequest, RemoveUserResponse>(validator, userContextProvider, logger, metricsRecorder)
{
    [OpenApiOperation(operationId: "RemoveUser", tags: ["User"],
        Summary = "Remove all assigned roles from a user at the specified site")]
    [OpenApiRequestBody("application/json", typeof(RemoveUserRequest), Required = true)]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, "application/json", typeof(RemoveUserRequest),
        Description = "Users roles removed for the specified site")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.BadRequest, "application/json",
        typeof(IEnumerable<ErrorMessageResponseItem>), Description = "The body of the request is invalid")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.NotFound, "application/json", typeof(string),
        Description = "User did not exist to be removed")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, "application/json",
        typeof(ErrorMessageResponseItem), Description = "Unauthorized request to a protected API")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.Forbidden, "application/json", typeof(ErrorMessageResponseItem),
        Description = "Request failed due to insufficient permissions")]
    [RequiresPermission(Permissions.ManageUsers, typeof(SiteFromBodyInspector))]
    [Function("RemoveUserFunction")]
    public override Task<IActionResult> RunAsync(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "user/remove")]
        HttpRequest req)
    {
        return base.RunAsync(req);
    }

    protected override async Task<ApiResult<RemoveUserResponse>> HandleRequest(RemoveUserRequest request,
        ILogger logger)
    {
        if (userContextProvider.UserPrincipal.Claims.GetUserEmail() == request.User)
        {
            return Failed(HttpStatusCode.BadRequest, "You cannot remove the currently logged in user.");
        }

        var result = await userService.RemoveUserAsync(request.User, request.Site);
        return result.Success
            ? Success(new RemoveUserResponse(request.User, request.Site))
            : Failed(HttpStatusCode.NotFound, result.Message);
    }
}
