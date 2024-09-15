using System.Collections.Generic;
using System.Threading.Tasks;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Nhs.Appointments.Api.Auth;
using Nhs.Appointments.Api.Json;
using Nhs.Appointments.Api.Models;
using Nhs.Appointments.Core;

namespace Nhs.Appointments.Api.Functions;

public class SetSiteAttributesFunction(ISiteService siteService, IValidator<SetSiteAttributesRequest> validator, IUserContextProvider userContextProvider, ILogger<SetSiteAttributesFunction> logger) 
    : BaseApiFunction<SetSiteAttributesRequest, EmptyResponse>(validator, userContextProvider, logger)
{
    [Function("SetSiteAttributesFunction")]
    public override Task<IActionResult> RunAsync(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "sites/{site}/attributes")] HttpRequest req)
    {
        return base.RunAsync(req);
    }

    protected override async Task<ApiResult<EmptyResponse>> HandleRequest(SetSiteAttributesRequest request, ILogger logger)
    {
        var result = await siteService.UpdateSiteAttributesAsync(request.Site, request.AttributeValues);
        return result ? Success(new EmptyResponse()) :
        Failed(System.Net.HttpStatusCode.NotFound, "The specified site was not found.");
    }

    protected override async Task<(bool requestRead, SetSiteAttributesRequest request)> ReadRequestAsync(HttpRequest req)
    {
        var site = req.HttpContext.GetRouteValue("site")?.ToString();
        var attributes = await JsonRequestReader.ReadRequestAsync<IEnumerable<AttributeValue>>(req.Body);
        return (true, new SetSiteAttributesRequest(site, attributes));
    }
}
