using System.Collections.Generic;
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
using Nhs.Appointments.Core;
using Nhs.Appointments.Core.Inspectors;

namespace Nhs.Appointments.Api.Functions;

public class SetSiteInformationForCitizensFunction(
    ISiteService siteService, 
    IValidator<SetSiteInformationForCitizensRequest> validator, 
    IUserContextProvider userContextProvider, 
    ILogger<SetSiteInformationForCitizensFunction> logger, 
    IMetricsRecorder metricsRecorder)
    : BaseApiFunction<SetSiteInformationForCitizensRequest, EmptyResponse>(validator, userContextProvider, logger, metricsRecorder)
{
    [OpenApiOperation(operationId: "SetSiteInformationForCitizens", tags: ["Sites"], Summary = "Set informationForCitizens value for a site")]
    [OpenApiRequestBody("application/json", typeof(SetSiteInformationForCitizensRequest), Required = true)]
    [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.OK, Description = "Site informationForCitizens value successfully saved")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.BadRequest, "application/json", typeof(IEnumerable<ErrorMessageResponseItem>), Description = "The body of the request is invalid")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.NotFound, "application/json", typeof(ApiResult<object>), Description = "Booking not found")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, "application/json", typeof(ErrorMessageResponseItem), Description = "Unauthorized request to a protected API")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.Forbidden, "application/json", typeof(ErrorMessageResponseItem), Description = "Request failed due to insufficient permissions")]
    [RequiresPermission(Permissions.ManageSite, typeof(SiteFromPathInspector))]
    [Function("SetSiteInformationForCitizensFunction")]
    public override Task<IActionResult> RunAsync(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "sites/{site}/informationForCitizens")] HttpRequest req)
    {
        return base.RunAsync(req);
    }

    protected override async Task<ApiResult<EmptyResponse>> HandleRequest(SetSiteInformationForCitizensRequest request, ILogger logger)
    {
        var result = await siteService.UpdateInformationForCitizens(request.Site, request.InformationForCitizens);
        return result.Success ? Success(new EmptyResponse()) : Failed(HttpStatusCode.NotFound, result.Message);
    }

    protected override async Task<(IReadOnlyCollection<ErrorMessageResponseItem> errors, SetSiteInformationForCitizensRequest request)> ReadRequestAsync(HttpRequest req)
    {
        var site = req.HttpContext.GetRouteValue("site")?.ToString();
        var (errors, request) = await JsonRequestReader.ReadRequestAsync<InformationForCitizensRequest>(req.Body);
        return (errors, new SetSiteInformationForCitizensRequest(site, request?.InformationForCitizens));
    }
}
