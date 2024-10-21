using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Extensions.Logging;
using Nhs.Appointments.Api.Auth;
using Nhs.Appointments.Api.Availability;
using Nhs.Appointments.Api.Models;
using System;
using System.Net;
using System.Threading.Tasks;

namespace Nhs.Appointments.Api.Functions;

public class SetAvailabilityFunction : BaseApiFunction<SetAvailabilityRequest, EmptyResponse>
{
    public SetAvailabilityFunction(IValidator<SetAvailabilityRequest> validator, IUserContextProvider userContextProvider, ILogger logger)
        : base(validator, userContextProvider, logger)
    {
    }

    [OpenApiOperation(operationId: "SetAvailability", tags: new[] {"Appointment Availability"}, Summary = "Set appointment availability")]
    [OpenApiRequestBody("text/json", typeof(SetAvailabilityRequest), Required = true)]
    [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.OK, Description = "Site availability successfully set or updated")]
    [RequiresPermission("availability:query", typeof(NoSiteRequestInspector))]
    [Function("SetAvailabilityFunction")]
    public override Task<IActionResult> RunAsync(HttpRequest req)
    {
        return base.RunAsync(req);
    }

    protected override Task<ApiResult<EmptyResponse>> HandleRequest(SetAvailabilityRequest request, ILogger logger)
    {
        throw new NotImplementedException();
    }
}

