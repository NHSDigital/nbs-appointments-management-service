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
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.OpenApi.Models;
using Nhs.Appointments.Api.Auth;

namespace Nhs.Appointments.Api.Functions;

public class QueryBookingByNhsNumberFunction : BaseApiFunction<QueryBookingByNhsNumberRequest, IEnumerable<Booking>>
{
    private readonly IBookingsService _bookingsService;

    public QueryBookingByNhsNumberFunction(
        IBookingsService bookingsService, 
        IValidator<QueryBookingByNhsNumberRequest> validator,
        IUserContextProvider userContextProvider,
        ILogger<QueryBookingByNhsNumberFunction> logger) : base(validator, userContextProvider, logger)
    {
        _bookingsService = bookingsService;
    }

    [OpenApiOperation(operationId: "QueryBookingByNhsNumber", tags: new [] {"Booking"}, Summary = "Query a booking by Nhs Number")]
    [OpenApiParameter("nhsNumber", Required = true, In = ParameterLocation.Query, Description = "The nhsNumber of the patients' bookings to retrieve")]
    [OpenApiSecurity("Api Key", SecuritySchemeType.ApiKey, Name = "Authorization", In = OpenApiSecurityLocationType.Header)]
    [OpenApiResponseWithBody(statusCode:HttpStatusCode.OK, contentType: "text/json", typeof(IEnumerable<Booking>), Description = "The bookings for patient with the provided nhsNumber")]
    [RequiresPermission("booking:query", typeof(NoSiteRequestInspector))]
    [Function("QueryBookingByNhsNumberReference")]
    public override Task<IActionResult> RunAsync(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "booking")] HttpRequest req)
    {
        return base.RunAsync(req);
    }

    protected override async Task<ApiResult<IEnumerable<Booking>>> HandleRequest(QueryBookingByNhsNumberRequest request, ILogger logger)
    {
        var booking = await _bookingsService.GetBookingByNhsNumber(request.nhsNumber);
        return Success(booking);
    }

    protected override Task<(bool requestRead, QueryBookingByNhsNumberRequest request)> ReadRequestAsync(HttpRequest req)
    {
        var nhsNumber = req.Query["nhsNumber"];
        return Task.FromResult<(bool requestRead, QueryBookingByNhsNumberRequest request)>((true, new QueryBookingByNhsNumberRequest(nhsNumber)));
    }
}