using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Nhs.Appointments.Api.Auth;
using Nhs.Appointments.Api.Json;
using Nhs.Appointments.Api.Models;
using Nhs.Appointments.Audit.Functions;
using Nhs.Appointments.Core;
using Nhs.Appointments.Core.Inspectors;

namespace Nhs.Appointments.Api.Functions;

public class CancelBookingFunction(
    IBookingWriteService bookingWriteService,
    IValidator<CancelBookingRequest> validator,
    IUserContextProvider userContextProvider,
    ILogger<CancelBookingFunction> logger,
    IMetricsRecorder metricsRecorder,
    ISiteService siteService)
    : BaseApiFunction<CancelBookingRequest, CancelBookingResponse>(validator, userContextProvider, logger,
        metricsRecorder)
{
    [OpenApiOperation(operationId: "CancelBooking", tags: ["Booking"], Summary = "Cancel a booking")]
    [OpenApiRequestBody("application/json", typeof(CancelBookingOpenApiRequest),
        Required = false,
        Example = typeof(CancelBookingOpenApiRequest))]
    [OpenApiParameter("bookingReference",
        Required = true,
        In = ParameterLocation.Path,
        Description = "The booking reference of the booking to cancel",
        Type = typeof(string))]
    [OpenApiParameter("site",
        Required = false,
        In = ParameterLocation.Query,
        Description =
            "The site at which the booking is scheduled. Optional; if provided an additional check will be made to validate that the booking is scheduled at the specified site.",
        Type = typeof(string))]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, "application/json", typeof(CancelBookingResponse),
        Description = "Booking successfully cancelled")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.BadRequest, "application/json",
        typeof(IEnumerable<ErrorMessageResponseItem>), Description = "The body of the request is invalid")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.NotFound, "application/json", typeof(string),
        Description =
            "A scheduled booking with a matching reference could not be found. Or, if site was provided, the booking's site did not match the site provided by the caller.")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, "application/json",
        typeof(ErrorMessageResponseItem), Description = "Unauthorized request to a protected API")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.Forbidden, "application/json", typeof(ErrorMessageResponseItem),
        Description = "Request failed due to insufficient permissions")]
    [RequiresPermission(Permissions.CancelBooking, typeof(SiteFromQueryStringInspector))]
    [RequiresAudit(typeof(SiteFromQueryStringInspector))]
    [Function("CancelBookingFunction")]
    public override Task<IActionResult> RunAsync(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "booking/{bookingReference}/cancel")]
        HttpRequest req)
    {
        return base.RunAsync(req);
    }

    protected override async Task<ApiResult<CancelBookingResponse>> HandleRequest(CancelBookingRequest request,
        ILogger logger)
    {
        var result = await bookingWriteService.CancelBooking(request.bookingReference, request.site,
            (CancellationReason)request.cancellationReason, request.additionalData);

        switch (result)
        {
            case BookingCancellationResult.Success:
                var response = new CancelBookingResponse(request.bookingReference, "cancelled");
                return Success(response);
            case BookingCancellationResult.NotFound:
                return Failed(HttpStatusCode.NotFound, "booking not found");
            default:
                throw new Exception($"Unexpected cancellation result status: {result}");
        }
    }

    protected override async Task<(IReadOnlyCollection<ErrorMessageResponseItem> errors, CancelBookingRequest request)>
        ReadRequestAsync(HttpRequest req)
    {
        var errors = new List<ErrorMessageResponseItem>();
        var bookingReference = req.HttpContext.GetRouteValue("bookingReference")?.ToString();
        var site = req.Query.ContainsKey("site") ? req.Query["site"].ToString() : string.Empty;

        if (!string.IsNullOrEmpty(site))
        {
            var matchedSite = await siteService.GetSiteByIdAsync(site);
            if (matchedSite == null)
            {
                errors.Add(new ErrorMessageResponseItem
                {
                    Property = "site",
                    Message = "The site identifier provided did not match any known site."
                });
            }
        }
            
        var cancellationReason = CancellationReason.CancelledByCitizen;
        var (errorsFromBody, payload) = await JsonRequestReader.ReadRequestAsync<CancelBookingRequest>(req.Body);

        errors.AddRange(errorsFromBody);
        
        if (errors.Count != 0)
        {
            return (errors, new CancelBookingRequest(bookingReference, site, cancellationReason, null));
        }
        if (payload?.cancellationReason != null)
        {
            cancellationReason = (CancellationReason)payload.cancellationReason;
        }

        var additionalData = payload?.additionalData;

        var requestModel = new CancelBookingRequest(bookingReference, site, cancellationReason, additionalData);

        return await Task.FromResult((errors, requestModel));
    }
}
