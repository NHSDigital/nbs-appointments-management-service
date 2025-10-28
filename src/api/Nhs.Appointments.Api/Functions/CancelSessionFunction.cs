using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Extensions.Logging;
using Nhs.Appointments.Api.Auth;
using Nhs.Appointments.Api.Models;
using Nhs.Appointments.Core;
using Nhs.Appointments.Core.Features;
using Nhs.Appointments.Core.Inspectors;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace Nhs.Appointments.Api.Functions;

public class CancelSessionFunction(
    IAvailabilityWriteService availabilityWriteService,
    IValidator<CancelSessionRequest> validator,
    IUserContextProvider userContextProvider,
    ILogger<CancelSessionFunction> logger,
    IMetricsRecorder metricsRecorder,
    IFeatureToggleHelper featureToggleHelper)
    : BaseApiFunction<CancelSessionRequest, EmptyResponse>(validator, userContextProvider, logger, metricsRecorder)
{
    [OpenApiOperation(operationId: "CancelSession", tags: ["Availability"], Summary = "Cancel a session")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, "application/json", typeof(CancelBookingResponse),
        Description = "Sessions successfully cancelled")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.BadRequest, "application/json",
        typeof(IEnumerable<ErrorMessageResponseItem>), Description = "The body of the request is invalid")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.NotFound, "application/json", typeof(string),
        Description = "Could not find session to cancel")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, "application/json",
        typeof(ErrorMessageResponseItem), Description = "Unauthorized request to a protected API")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.Forbidden, "application/json", typeof(ErrorMessageResponseItem),
        Description = "Request failed due to insufficient permissions")]
    [RequiresPermission(Permissions.SetupAvailability, typeof(SiteFromBodyInspector))]
    [Function("CancelSessionFunction")]
    public override Task<IActionResult> RunAsync(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "session/cancel")]
        HttpRequest req)
    {
        return base.RunAsync(req);
    }

    protected override async Task<ApiResult<EmptyResponse>> HandleRequest(CancelSessionRequest request, ILogger logger)
    {
            await availabilityWriteService.CancelSession(
                request.Site,
                request.Date,
                request.From,
                request.Until,
                request.Services,
                request.SlotLength,
                request.Capacity
            );

            return Success(new EmptyResponse());
    }
}
