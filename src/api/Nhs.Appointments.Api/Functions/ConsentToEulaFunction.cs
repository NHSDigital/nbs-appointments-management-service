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
using Nhs.Appointments.Api.Auth;
using Nhs.Appointments.Api.Models;
using Nhs.Appointments.Core;

namespace Nhs.Appointments.Api.Functions;

public class ConsentToEulaFunction(IEulaService eulaService, IValidator<ConsentToEulaRequest> validator, IUserContextProvider userContextProvider, ILogger<SetUserRolesFunction> logger, IMetricsRecorder metricsRecorder)
    : BaseApiFunction<ConsentToEulaRequest, EmptyResponse>(validator, userContextProvider, logger, metricsRecorder)
{
    [OpenApiOperation(operationId: "ConsentToEula", tags: ["Eula"], Summary = "Confirm a user's consent to a specific EULA version.")]
    [OpenApiRequestBody("application/json", typeof(ConsentToEulaRequest), Required = true)]
    [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.OK, Description = "User's consent successfully recorded.")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.BadRequest, "application/json", typeof(IEnumerable<ErrorMessageResponseItem>), Description = "The body of the request is invalid")]
    [Function("ConsentToEula")]
    public override Task<IActionResult> RunAsync(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "eula/consent")] HttpRequest req)
    {
        return base.RunAsync(req);
    }

    protected override async Task<ApiResult<EmptyResponse>> HandleRequest(ConsentToEulaRequest request, ILogger logger)
    {
        var latestEulaVersion = await eulaService.GetEulaVersionAsync();

        if (request.versionDate != latestEulaVersion.VersionDate)
        {
            return Failed(HttpStatusCode.BadRequest, "The EULA version date provided is not the latest EULA version.");
        }

        await eulaService.ConsentToEula(request.userId);
        
        return Success(new EmptyResponse());
    }
}


