using System;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Extensions.Logging;
using Nhs.Appointments.Api.Auth;
using Nhs.Appointments.Api.Json;
using Nhs.Appointments.Api.Models;
using Nhs.Appointments.Core;
using Nhs.Appointments.Core.Features;
using Nhs.Appointments.Core.Inspectors;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace Nhs.Appointments.Api.Functions;

public class EditSessionFunction(
    IValidator<EditSessionRequest> validator,
    IUserContextProvider userContextProvider,
    ILogger<EditSessionFunction> logger,
    IMetricsRecorder metricsRecorder,
    IAvailabilityWriteService availabilityWriteService,
    IFeatureToggleHelper featureToggleHelper)
    : BaseApiFunction<EditSessionRequest, SessionModificationResult>(validator, userContextProvider, logger,
        metricsRecorder)
{
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.BadRequest, "application/json",
        typeof(ErrorMessageResponseItem), Description = "The body of the request is invalid")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, "application/json",
        typeof(ErrorMessageResponseItem), Description = "Unauthorized request to a protected API")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.UnprocessableEntity, "application/json", typeof(string),
        Description = "Update to session failed due to no matching session or no day document")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.Forbidden, "application/json", typeof(ErrorMessageResponseItem),
        Description = "Request failed due to insufficient permissions")]
    [RequiresPermission(Permissions.SetupAvailability, typeof(SiteFromBodyInspector))]
    [Function("EditSessionFunction")]
    public override async Task<IActionResult> RunAsync(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "session/edit")]
        HttpRequest req)
    {
        return await featureToggleHelper.IsFeatureEnabled(Flags.ChangeSessionUpliftedJourney)
            ? await base.RunAsync(req)
            : ProblemResponse(HttpStatusCode.NotImplemented, null);
    }

    protected override async Task<ApiResult<SessionModificationResult>> HandleRequest(EditSessionRequest request, ILogger logger)
    {
        if (request.SessionMatcher.IsWildcard)
        {
            throw new NotImplementedException("Wildcard journey matcher is not implemented and needs re-developing when required.");
        }
        
        var result = await availabilityWriteService.EditOrCancelSessionAsync(
            request.Site,
            request.From,
            request.To,
            request.SessionMatcher.Session,
            request.SessionReplacement,
            request.CancelUnsupportedBookings);

        return result.UpdateSuccessful
            ? Success(result)
            : Failed(HttpStatusCode.UnprocessableContent, result.Message);
    }

    protected override async Task<(IReadOnlyCollection<ErrorMessageResponseItem> errors, EditSessionRequest request)> ReadRequestAsync(HttpRequest req)
    {
        var (errors, payload) = await JsonRequestReader.ReadRequestAsync<EditSessionRequest>(req.Body);

        if (errors.Count > 0)
        {
            return (errors, null);
        }

        return (errors, payload);
    }
}
