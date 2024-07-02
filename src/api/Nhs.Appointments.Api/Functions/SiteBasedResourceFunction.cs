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
    private readonly IUserService _userService;
    protected SiteBasedResourceFunction(
        IUserService userService,
        IValidator<SiteBasedResourceRequest> validator, 
        IUserContextProvider userContextProvider,
        ILogger logger) : base(validator, userContextProvider, logger)
    {
        _userService = userService;
    }

    protected override Task<(bool requestRead, SiteBasedResourceRequest request)> ReadRequestAsync(HttpRequest req)
    {
        var site = req.Query["site"];        
        return Task.FromResult<(bool requestRead, SiteBasedResourceRequest request)>((true, new SiteBasedResourceRequest(site)));
    }    
}
