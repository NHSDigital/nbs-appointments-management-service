using FluentValidation;
using Microsoft.AspNetCore.Http;
using Nhs.Appointments.Api.Auth;
using Nhs.Appointments.Core;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Nhs.Appointments.Api.Models;

namespace Nhs.Appointments.Api.Functions;

public abstract class SiteBasedResourceFunction<TResponse> : BaseApiFunction<SiteBasedResourceRequest, TResponse>
{
    private readonly IUserSiteAssignmentService _userSiteAssignmentService;
    protected SiteBasedResourceFunction(
        IUserSiteAssignmentService userSiteAssignmentService,
        IValidator<SiteBasedResourceRequest> validator, 
        IUserContextProvider userContextProvider,
        ILogger logger) : base(validator, userContextProvider, logger)
    {
        _userSiteAssignmentService = userSiteAssignmentService;
    }

    protected override Task<(bool requestRead, SiteBasedResourceRequest request)> ReadRequestAsync(HttpRequest req)
    {
        var site = req.Query["site"];
        var forUser = req.Query.ContainsKey("user");
        return Task.FromResult<(bool requestRead, SiteBasedResourceRequest request)>((true, new SiteBasedResourceRequest(site, forUser)));
    }

    protected async Task<string> GetSiteFromRequestAsync(SiteBasedResourceRequest request)
    {        
        if (request.ForUser)
        {
            var userEmail = Principal.Claims.GetUserEmail();
            return await _userSiteAssignmentService.GetSiteIdForUserByEmailAsync(userEmail);
        }
        return request.Site;
    }
}
