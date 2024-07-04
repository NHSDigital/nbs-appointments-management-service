using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Nhs.Appointments.Api.Models;
using Nhs.Appointments.Core;
using System.Text.RegularExpressions;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.OpenApi.Models;
using Nhs.Appointments.Api.Auth;

namespace Nhs.Appointments.Api.Functions;

public class SetSiteConfiguration :  BaseApiFunction<SiteConfiguration, EmptyResponse> 
{
    private readonly ISiteConfigurationService _siteConfigurationService;
    private readonly IScheduleService _scheduleService;

    public SetSiteConfiguration(ISiteConfigurationService siteConfigurationService, 
        IScheduleService scheduleService,
        IValidator<SiteConfiguration> validator,
        IUserContextProvider userContextProvider,
        ILogger<SetSiteConfiguration> logger) : base(validator, userContextProvider, logger)
    {
        _siteConfigurationService = siteConfigurationService;
        _scheduleService = scheduleService;
    }
    
    [OpenApiOperation(operationId: "SetSiteConfiguration", tags: new [] {"Site Configuration"}, Summary = "Set the site configuration")]
    [OpenApiRequestBody("text/json", typeof(SiteConfiguration))]
    [OpenApiSecurity("Api Key", SecuritySchemeType.ApiKey, Name = "Authorization", In = OpenApiSecurityLocationType.Header)]
    [OpenApiResponseWithoutBody(statusCode:HttpStatusCode.OK, Description = "Site configuration successfully created")]
    [OpenApiResponseWithBody(statusCode:HttpStatusCode.BadRequest, contentType: "text/json", typeof(IEnumerable<ErrorMessageResponseItem>),  Description = "The body of the request is invalid")]
    [RequiresPermission("site:set-config")]
    [Function("SetSiteConfiguration")]
    public override Task<IActionResult> RunAsync(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "site-configuration")] HttpRequest req)
    {
        return base.RunAsync(req);
    }

    protected override async Task<ApiResult<EmptyResponse>> HandleRequest(SiteConfiguration request, ILogger logger)
    {
        request.InformationForCitizen = SanitizeInput(request.InformationForCitizen);
        await _siteConfigurationService.PutSiteConfigurationAsync(request);
        var disabledServiceTypes = request.ServiceConfiguration
            .Where(x => x.Enabled == false)
            .Select(x => x.Code);
        if (disabledServiceTypes.Any())
        {
            await _scheduleService.UpdateDisabledServiceTypes(request.Site, disabledServiceTypes);
        }
        return Success();
    }
    
    private static string SanitizeInput(string input)
    {
        return input == null ? null : Regex.Replace(input, @"[^ a-zA-Z0-9-,.:]", "");
    }
}
