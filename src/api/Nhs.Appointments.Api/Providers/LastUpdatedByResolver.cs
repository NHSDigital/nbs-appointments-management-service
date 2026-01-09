using Microsoft.Extensions.Options;
using Nhs.Appointments.Core.Users;
using Nhs.Appointments.Persistance;
using System.Security.Claims;

namespace Nhs.Appointments.Api.Providers;

public class LastUpdatedByResolver(
    IUserContextProvider userContextProvider, 
    IOptions<ApplicationOptions> options
) : ILastUpdatedByResolver
{
    private readonly ApplicationOptions _settings = options.Value;
    public string GetLastUpdatedBy(){
        var user = userContextProvider.UserPrincipal?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return user ?? _settings.ApplicationName;
    }
}
