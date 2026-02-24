using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Extensions.Logging;
using Nhs.Appointments.Api.Models;
using Nhs.Appointments.Core.Bookings;
using Nhs.Appointments.Core.Features;
using Nhs.Appointments.Core.Users;
using System;
using System.Net;
using System.Threading.Tasks;

namespace Nhs.Appointments.Api.Functions.HttpFunctions;

public class ProposeCancelDateRangeFunction(
    IBookingAvailabilityStateService bookingAvailabilityStateService,
    IValidator<ProposeCancelDateRangeRequest> validator,
    IUserContextProvider userContextProvider,
    ILogger<ProposeCancelDateRangeFunction> logger,
    IMetricsRecorder metricsRecorder,
    IFeatureToggleHelper featureToggleHelper)
    : BaseApiFunction<ProposeCancelDateRangeRequest, ProposeCancelDateRangeResponse>(
        validator,
        userContextProvider,
        logger,
        metricsRecorder
    )
{
    [OpenApiOperation(operationId: "ProposeCancelDateRange", tags: ["availability"],
        Summary = "Get proposed changes for cancelling availability in a given date range.")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, "application/json",
        typeof(EmptyResponse),
        Description = "Proposal of how many sessions and bookings will be cancelled")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, "application/json",
        typeof(ErrorMessageResponseItem), Description = "Unauthorized request to a protected API")]
    [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.NotImplemented,
        Description = "The cancel date range proposal function is disabled or not available.")]
    [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.BadRequest, Description = "The body of the request is invalid.")]
    [Function("ProposeCancelDateRangeFunction")]
    public override async Task<IActionResult> RunAsync(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "availability/propose-cancel-date-range")]
        HttpRequest req)
    {
        return await featureToggleHelper.IsFeatureEnabled(Flags.CancelADateRange)
            ? await base.RunAsync(req)
            : ProblemResponse(HttpStatusCode.NotImplemented, null);
    }

    protected override async Task<ApiResult<ProposeCancelDateRangeResponse>> HandleRequest(ProposeCancelDateRangeRequest request, ILogger logger)
    {
        var (sessionCount, bookingsCount) = await bookingAvailabilityStateService.GenerateCancelDateRangeProposalActionMetricsAsync(request.Site, request.From, request.To);
        return ApiResult<ProposeCancelDateRangeResponse>.Success(new ProposeCancelDateRangeResponse(sessionCount, bookingsCount));
    }
}
