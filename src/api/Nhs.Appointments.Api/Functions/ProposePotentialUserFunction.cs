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

namespace Nhs.Appointments.Api.Functions;

public class ProposePotentialUserFunction(
    IUserService userService,
    IValidator<ProposePotentialUserRequest> validator,
    IUserContextProvider userContextProvider,
    ILogger<ProposePotentialUserFunction> logger,
    IMetricsRecorder metricsRecorder
) : BaseApiFunction<ProposePotentialUserRequest, ProposePotentialUserResponse>(validator, userContextProvider, logger,
    metricsRecorder)
{
    [OpenApiOperation("ProposePotentialUser", "user/propose-potential",
        Summary = "Gets the current status of a potential new user. Used to inform the user creation process.")]
    [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(ProposePotentialUserResponse),
        Description = "The response containing the current status of a potential new user.")]
    [Function("ProposePotentialUserFunction")]
    public override Task<IActionResult> RunAsync(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "user/propose-potential")]
        HttpRequest req) =>
        base.RunAsync(req);

    protected override async Task<ApiResult<ProposePotentialUserResponse>> HandleRequest(
        ProposePotentialUserRequest request,
        ILogger logger)
    {
        var userIdentityStatus = await userService.GetUserIdentityStatusAsync(request.SiteId, request.UserId);

        return ApiResult<ProposePotentialUserResponse>.Success(new ProposePotentialUserResponse
        {
            ExtantInSite = userIdentityStatus.ExtantInSite,
            ExtantInIdentityProvider = userIdentityStatus.ExtantInIdentityProvider,
            MeetsWhitelistRequirements = userIdentityStatus.MeetsWhitelistRequirements,
            IdentityProvider = userIdentityStatus.IdentityProvider
        });
    }
}
