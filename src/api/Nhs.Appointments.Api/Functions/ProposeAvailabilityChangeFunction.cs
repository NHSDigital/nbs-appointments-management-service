using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Extensions.Logging;
using Nhs.Appointments.Api.Json;
using Nhs.Appointments.Api.Models;
using Nhs.Appointments.Core;
using Nhs.Appointments.Core.Features;

namespace Nhs.Appointments.Api.Functions;

public class ProposeAvailabilityChangeFunction(
    IBookingAvailabilityStateService bookingAvailabilityStateService,
    IValidator<AvailabilityChangeProposalRequest> validator,
    IUserContextProvider userContextProvider,
    ILogger<ProposeAvailabilityChangeFunction> logger,
    IMetricsRecorder metricsRecorder,
    IFeatureToggleHelper featureToggleHelper)
    : BaseApiFunction<AvailabilityChangeProposalRequest, AvailabilityChangeProposalResponse>(
        validator,
        userContextProvider,
        logger,
        metricsRecorder
    )
{
    [OpenApiOperation(operationId: "GetAvailabilityChangeProposal", tags: ["availability"],
        Summary = "Get proposed changes for availability update.")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, "application/json",
        typeof(AvailabilityChangeProposalResponse),
        Description = "Proposal of how many bookings will be rehomed/unassigned")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, "application/json",
        typeof(ErrorMessageResponseItem), Description = "Unauthorized request to a protected API")]
    [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.NotFound,
        Description = "The availability change proposal function is disabled or not available.")]
    [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.BadRequest,
        Description = "No matching session was found for the provided request parameters.")]
    [Function("AvailabilityChangeProposalFunction")]
    public override Task<IActionResult> RunAsync(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "availability/propose-edit")]
        HttpRequest req)
    {
        return base.RunAsync(req);
    }

    protected override async Task<ApiResult<AvailabilityChangeProposalResponse>> HandleRequest(
        AvailabilityChangeProposalRequest request,
        ILogger logger)
    {
        if (await featureToggleHelper.IsFeatureEnabled(Flags.ChangeSessionUpliftedJourney))
        {
            var recalculations = await bookingAvailabilityStateService.GenerateSessionProposalActionMetrics(
                request.Site,
                request.From.ToDateTime(TimeOnly.MinValue),
                request.To.ToDateTime(new TimeOnly(23, 59, 59)),
                request.SessionMatcher.Session,
                request.SessionReplacement,
                request.SessionMatcher.IsWildcard);

            if (recalculations.MatchingSessionNotFound)
            {
                return ApiResult<AvailabilityChangeProposalResponse>.Failed(
                    HttpStatusCode.BadRequest, "Matching session was not found"
                );
            }

            return ApiResult<AvailabilityChangeProposalResponse>.Success(
                new AvailabilityChangeProposalResponse(
                    recalculations.NewlySupportedBookingsCount,
                    recalculations.NewlyOrphanedBookingsCount
                )
            );
        }

        return ApiResult<AvailabilityChangeProposalResponse>.Failed(
            HttpStatusCode.NotFound, "Availability change proposal function is not available."
        );
    }

    protected override async
        Task<(IReadOnlyCollection<ErrorMessageResponseItem> errors, AvailabilityChangeProposalRequest request)>
        ReadRequestAsync(HttpRequest req)
    {
        var (errors, payload) = await JsonRequestReader.ReadRequestAsync<AvailabilityChangeProposalRequest>(req.Body);

        if (errors.Count > 0)
        {
            return (errors, null);
        }

        return (errors, payload);
    }
}
