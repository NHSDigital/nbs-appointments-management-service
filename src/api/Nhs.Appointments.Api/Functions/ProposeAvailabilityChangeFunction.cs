using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Nhs.Appointments.Api.Models;
using Nhs.Appointments.Core;
using System.Net;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Nhs.Appointments.Api.Functions;
public class ProposeAvailabilityChangeFunction(
    IBookingAvailabilityStateService bookingAvailabilityStateService,
    IValidator<AvailabilityChangeProposalRequest> validator,
    IUserContextProvider userContextProvider,
    ILogger<ProposeAvailabilityChangeFunction> logger,
    IMetricsRecorder metricsRecorder) 
    : BaseApiFunction<AvailabilityChangeProposalRequest, AvailabilityChangeProposalResponse>(
        validator, 
        userContextProvider,
        logger, 
        metricsRecorder
    )
{

    [OpenApiOperation(operationId: "GetAvailabilityChangeProposal", tags: ["availability"],
        Summary = "Get proposed changes for availability update.")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, "application/json", typeof(AvailabilityChangeProposalResponse),
        Description = "Proposal of how many bookings will be rehomed/unassigned")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, "application/json",
        typeof(ErrorMessageResponseItem), Description = "Unauthorized request to a protected API")]
    [Function("AvailabilityChangeProposalFunction")]
    public override Task<IActionResult> RunAsync(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "availability/propose-edit")]
        HttpRequest req)
    {
        return base.RunAsync(req);
    }

    protected override async Task<ApiResult<AvailabilityChangeProposalResponse>> HandleRequest(AvailabilityChangeProposalRequest request,
        ILogger logger)
    {
        var recalculations = await bookingAvailabilityStateService.BuildRecalculations(
            request.Site, 
            request.FromDate, 
            request.ToDate,
            request.SessionMatcher,
            request.SessionReplacement);

        if (recalculations.MatchingSessionNotFound)
        {
            return ApiResult<AvailabilityChangeProposalResponse>.Failed(
                HttpStatusCode.BadRequest, "Matching session was not found"
            );
        }

        return ApiResult<AvailabilityChangeProposalResponse>.Success(
            new AvailabilityChangeProposalResponse(
                recalculations.SupportedBookingsCount, 
                recalculations.UnsupportedBookingsCount
            )
        );
    }
}
