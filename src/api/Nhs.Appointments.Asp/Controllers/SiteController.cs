using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Nhs.Appointments.Core;
using Nhs.Appointments.Http;
using Nhs.Appointments.Http.Extensions;

namespace Nhs.Appointments.Asp.Controllers;

[ApiController]
[Route("sites")]
public class SiteController(ISiteService siteService, IValidator<GetSitesByAreaRequest> siteByAreaValidator) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetSitesByArea(
        [FromQuery(Name = "long")] double longitude, 
        [FromQuery(Name = "lat")] double latitude, 
        int searchRadius,
        int maxRecords,
        string? services,
        string? from,
        string? until,
        string? accessNeeds = null,
        bool ignoreCache = false)
    {
        var request = new GetSitesByAreaRequest(
            longitude, 
            latitude, 
            searchRadius, 
            maxRecords, 
            accessNeeds is not null ? accessNeeds.Split(',') : [], 
            ignoreCache, 
            (services?.Split(',') ?? null)!, 
            from!, 
            until!);
        
        var validationResult = await siteByAreaValidator.RunValidator(request);

        if (validationResult.Any())
        {
            return BadRequest(validationResult);
        }

        SiteSupportsServiceFilter? siteSupportsServiceFilter = null;

        //if all 3 params are provided correctly, use the SiteSupportsServiceFilter
        if (request.Services is { Length: 1 } && request.FromDate != null && request.UntilDate != null)
        {
            siteSupportsServiceFilter = new SiteSupportsServiceFilter(request.Services.Single(), request.FromDate.Value, request.UntilDate.Value);
        }

        var sites = await siteService.FindSitesByArea(request.Longitude, request.Latitude, request.SearchRadius,
            request.MaximumRecords, request.AccessNeeds, request.IgnoreCache, siteSupportsServiceFilter);
        return Ok(ApiResult<IEnumerable<SiteWithDistance>>.Success(sites));
    }
    
    [HttpGet("{site}/meta")]
    public async Task<IActionResult> GetMetaData(string site)
    {
        const string scope = "site_details";
        var requestedSite = await siteService.GetSiteByIdAsync(site, scope);
        if (requestedSite != null)
        {
            var patientInformation = requestedSite.Accessibilities.Any()
                ? requestedSite.Accessibilities?.FirstOrDefault(a => a.Id == $"{scope}/info_for_citizen")?.Value ?? string.Empty
                : string.Empty;
            Ok(ApiResult<GetSiteMetaDataResponse>.Success(
                new GetSiteMetaDataResponse(requestedSite.Name, patientInformation)));
        }
        
        return NotFound("No site configuration was found for the specified site");
    }
}
