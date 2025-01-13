using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Nhs.Appointments.Core;
using System.Collections.Generic;
using System.Net;
using Nhs.Appointments.Api.Models;
using FluentValidation;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.OpenApi.Models;
using Nhs.Appointments.Api.Auth;
using Nhs.Appointments.Core.Inspectors;

namespace Nhs.Appointments.Api.Functions;

public class QueryBookingByNhsNumberFunction(IBookingsService bookingsService, IValidator<QueryBookingByNhsNumberRequest> validator, IUserContextProvider userContextProvider, ILogger<QueryBookingByNhsNumberFunction> logger, IMetricsRecorder metricsRecorder)
    : BaseApiFunction<QueryBookingByNhsNumberRequest, IEnumerable<Booking>>(validator, userContextProvider, logger, metricsRecorder)
{

    [OpenApiOperation(operationId: "QueryBookingByNhsNumber", tags: ["Booking"], Summary = "Query bookings by Nhs Number")]
    [OpenApiParameter("nhsNumber", Required = true, In = ParameterLocation.Query, Description = "The nhsNumber of the patients' bookings to retrieve")]
    [OpenApiResponseWithBody(statusCode:HttpStatusCode.OK, "application/json", typeof(IEnumerable<Booking>), Description = "A list of bookings for patient with the provided nhsNumber")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.BadRequest, "application/json", typeof(ErrorMessageResponseItem), Description = "The body of the request is invalid")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, "application/json", typeof(ErrorMessageResponseItem), Description = "Unauthorized request to a protected API")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.Forbidden, "application/json", typeof(ErrorMessageResponseItem), Description = "Request failed due to insufficient permissions")]
    [RequiresPermission("booking:query", typeof(NoSiteRequestInspector))]
    [Function("QueryBookingByNhsNumberReference")]
    public override Task<IActionResult> RunAsync(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "booking")] HttpRequest req)
    {
        return base.RunAsync(req);
    }

    protected override async Task<ApiResult<IEnumerable<Booking>>> HandleRequest(QueryBookingByNhsNumberRequest request, ILogger logger)
    {
        var booking = await bookingsService.GetBookingByNhsNumber(request.nhsNumber);
        return Success(booking);
    }

    protected override Task<(IReadOnlyCollection<ErrorMessageResponseItem> errors, QueryBookingByNhsNumberRequest request)> ReadRequestAsync(HttpRequest req)
    {
        var errors = new List<ErrorMessageResponseItem>();
        var nhsNumber = req.Query["nhsNumber"];
        if (string.IsNullOrEmpty(nhsNumber))
            errors.Add(new ErrorMessageResponseItem { Property = "nhsNumber", Message = "You must provide an nhsNumber" });
        return Task.FromResult<(IReadOnlyCollection<ErrorMessageResponseItem> errors, QueryBookingByNhsNumberRequest request)>((errors.AsReadOnly(), new QueryBookingByNhsNumberRequest(nhsNumber)));
    }
}