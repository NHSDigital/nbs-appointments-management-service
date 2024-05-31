using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Nhs.Appointments.Core;
using Nhs.Appointments.Api.Models;
using System.Collections.Generic;
using System.Net;
using FluentValidation;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.OpenApi.Models;
using Nhs.Appointments.Api.Auth;

namespace Nhs.Appointments.Api.Functions;

public class FindSitesByPostcodeFunction : BaseApiFunction<FindSitesByPostCodeRequest, IEnumerable<Site>>   
{
    private readonly IPostcodeLookupService _postcodeLookupService;
    private readonly ISiteSearchService _siteSearchService;

    public FindSitesByPostcodeFunction(
        IPostcodeLookupService postcodeLookupService, 
        ISiteSearchService siteSearchService,
        IValidator<FindSitesByPostCodeRequest> validator,
        IUserContextProvider userContextProvider,
        ILogger<FindSitesByPostcodeFunction> logger) : base(validator, userContextProvider, logger)
    {
        _postcodeLookupService = postcodeLookupService;
        _siteSearchService = siteSearchService;
    }

    [OpenApiOperation(operationId: "FindSitesByPostcode", tags: new [] {"Sites"}, Summary = "Find sites by postcode")]
    [OpenApiParameter("postcode", Required = true, In = ParameterLocation.Query, Description = "The postcode to find sites for")]
    [OpenApiSecurity("Api Key", SecuritySchemeType.ApiKey, Name = "Authorization", In = OpenApiSecurityLocationType.Header)]
    [OpenApiResponseWithBody(statusCode:HttpStatusCode.OK, contentType: "text/json", typeof(IEnumerable<Site>), Description = "The available sites at the given postcode")]
    [OpenApiResponseWithBody(statusCode:HttpStatusCode.BadRequest, contentType: "text/json", typeof(IEnumerable<ErrorMessageResponseItem>),  Description = "The body of the request is invalid" )]
    [Function("FindSitesByPostcodeFunction")]
    public override Task<IActionResult> RunAsync(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "sites")] HttpRequest req)
    {
        return base.RunAsync(req);
    }

    protected override async Task<ApiResult<IEnumerable<Site>>> HandleRequest(FindSitesByPostCodeRequest request, ILogger logger)
    {
        var geoLocation = await _postcodeLookupService.GeolocationFromPostcode(request.postCode);
        var nearbySites = await _siteSearchService.FindSitesByArea(geoLocation.longitude, geoLocation.latitude, 3, 10);
        return Success(nearbySites);
    }

    protected override Task<(bool requestRead, FindSitesByPostCodeRequest request)> ReadRequestAsync(HttpRequest req)
    {
        var postcode = req.Query["postcode"];
        return Task.FromResult<(bool requestRead, FindSitesByPostCodeRequest request)>((true, new FindSitesByPostCodeRequest(postcode)));
    }
}

