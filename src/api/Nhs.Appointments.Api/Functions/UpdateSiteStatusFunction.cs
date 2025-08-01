using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Extensions.Logging;
using Nhs.Appointments.Api.Auth;
using Nhs.Appointments.Api.Json;
using Nhs.Appointments.Api.Models;
using Nhs.Appointments.Core;
using Nhs.Appointments.Core.Features;
using Nhs.Appointments.Core.Inspectors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Nhs.Appointments.Api.Functions;

public class UpdateSiteStatusFunction(
    ISiteService siteService,
    IValidator<SetSiteStatusRequest> validator,
    ILogger<UpdateSiteStatusFunction> logger,
    IUserContextProvider userContextProvider,
    IMetricsRecorder metricsRecorder,
    IFeatureToggleHelper featureToggleHelper) : BaseApiFunction<SetSiteStatusRequest, EmptyResponse>(validator, userContextProvider, logger, metricsRecorder)
{
    private readonly ILogger<UpdateSiteStatusFunction> _logger;

    [OpenApiOperation(operationId: "Set Site Status", tags: ["SiteStatus"], Summary = "Set a site's status to online or offline")]
    [OpenApiRequestBody("application/json", typeof(SetSiteStatusRequest), Required = true)]
    [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.OK, Description = "Site status successfully set or updated")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.BadRequest, "application/json",
        typeof(ErrorMessageResponseItem), Description = "The body of the request is invalid")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, "application/json",
        typeof(ErrorMessageResponseItem), Description = "Unauthorized request to a protected API")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.Forbidden, "application/json", typeof(ErrorMessageResponseItem),
        Description = "Request failed due to insufficient permissions")]
    [RequiresPermission(Permissions.ManageSite, typeof(SiteFromBodyInspector))]
    [Function("UpdateSiteStatusFunction")]
    public override async Task<IActionResult> RunAsync([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "site-status")] HttpRequest req)
    {
        return await featureToggleHelper.IsFeatureEnabled(Flags.SiteStatus)
            ? await base.RunAsync(req)
            : ProblemResponse(HttpStatusCode.NotImplemented, null);
    }

    protected override Task<ApiResult<EmptyResponse>> HandleRequest(SetSiteStatusRequest request, ILogger logger)
    {
        throw new NotImplementedException();
    }

    protected override async Task<(IReadOnlyCollection<ErrorMessageResponseItem> errors, SetSiteStatusRequest request)> ReadRequestAsync(HttpRequest req)
    {
        var (errors, payload) = await JsonRequestReader.ReadRequestAsync<SetSiteStatusRequest>(req.Body, true);

        return errors.Any()
            ? (errors, payload)
            : (errors, new SetSiteStatusRequest(payload.site, payload.status));
    }
}
