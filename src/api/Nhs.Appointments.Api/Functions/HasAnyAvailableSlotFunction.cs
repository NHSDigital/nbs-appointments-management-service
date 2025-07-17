using System;
using System.Collections.Concurrent;
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
using Nhs.Appointments.Api.Availability;
using Nhs.Appointments.Api.Json;
using Nhs.Appointments.Api.Models;
using Nhs.Appointments.Core;
using Nhs.Appointments.Core.Inspectors;

namespace Nhs.Appointments.Api.Functions;

public class HasAnyAvailableSlotFunction(
    IBookingAvailabilityStateService bookingAvailabilityStateService,
    IValidator<HasAnyAvailableSlotRequest> validator,
    IUserContextProvider userContextProvider,
    ILogger<HasAnyAvailableSlotFunction> logger,
    IMetricsRecorder metricsRecorder)
    : BaseApiFunction<HasAnyAvailableSlotRequest, HasAnyAvailableSlotResponse>(validator, userContextProvider, logger,
        metricsRecorder)
{
    [OpenApiOperation(operationId: "HasAnyAvailableSlot", tags: ["Availability"],
        Summary = "Check whether a site has any available slot for the service within the date range")]
    [OpenApiRequestBody("application/json", typeof(HasAnyAvailableSlotRequest), Required = true)]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, "application/json",
        typeof(HasAnyAvailableSlotResponseItem[]),
        Description = "Whether the site has an available slot")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.BadRequest, "application/json",
        typeof(IEnumerable<ErrorMessageResponseItem>), Description = "The body of the request is invalid")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, "application/json",
        typeof(ErrorMessageResponseItem), Description = "Unauthorized request to a protected API")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.Forbidden, "application/json", typeof(ErrorMessageResponseItem),
        Description = "Request failed due to insufficient permissions")]
    [RequiresPermission(Permissions.QueryAvailability, typeof(MultiSiteBodyRequestInspector))]
    [Function("HasAnyAvailableSlotFunction")]
    public override Task<IActionResult> RunAsync(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "has-any-available-slot")]
        HttpRequest req)
    {
        return base.RunAsync(req);
    }

    protected override async Task<ApiResult<HasAnyAvailableSlotResponse>> HandleRequest(
        HasAnyAvailableSlotRequest request,
        ILogger logger)
    {
        var concurrentResults = new ConcurrentBag<HasAnyAvailableSlotResponseItem>();
        var response = new HasAnyAvailableSlotResponse();

        await Parallel.ForEachAsync(request.Sites, async (site, _) =>
        {
            var (hasAnyAvailableSlot, shortCircuited) =
                await bookingAvailabilityStateService.HasAnyAvailableSlot(request.Service, site, request.From, request.Until);
            concurrentResults.Add(new HasAnyAvailableSlotResponseItem(site, hasAnyAvailableSlot, shortCircuited));
        });

        if (concurrentResults.All(x => x.shortCircuited))
        {
            logger.LogInformation("HasAnyAvailableSlot - short-circuited successfully for all {SitesLength} sites", request.Sites.Length);
        }
        else
        {
            logger.LogInformation("HasAnyAvailableSlot - At least one site did not short-circuit");
        }

        response.AddRange(concurrentResults.Where(r => r is not null).OrderBy(r => r.site));
        return Success(response);
    }

    protected override async
        Task<(IReadOnlyCollection<ErrorMessageResponseItem> errors, HasAnyAvailableSlotRequest request)>
        ReadRequestAsync(HttpRequest req)
    {
        var request = default(HasAnyAvailableSlotRequest);
        if (req.Body.Length > 0)
        {
            var (errors, payload) =
                await JsonRequestReader.ReadRequestAsync<HasAnyAvailableSlotRequest>(req.Body, true);
            if (errors.Any())
            {
                return (errors, null);
            }

            request = payload;
        }

        return (ErrorMessageResponseItem.None,
            new HasAnyAvailableSlotRequest(request.Sites, request.Service, request.From, request.Until));
    }
}
