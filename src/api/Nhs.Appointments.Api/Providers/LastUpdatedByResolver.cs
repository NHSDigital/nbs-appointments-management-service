using Microsoft.Extensions.Options;
using Nhs.Appointments.Core.Extensions;
using Nhs.Appointments.Core.Users;
using Nhs.Appointments.Persistance;

namespace Nhs.Appointments.Api.Providers;

public class LastUpdatedByResolver(
    IUserContextProvider userContextProvider, 
    IOptions<ApplicationOptions> options
) : ILastUpdatedByResolver
{
    private readonly ApplicationOptions _settings = options.Value;
    public string GetLastUpdatedBy(){
        var user = userContextProvider.UserPrincipal?.Claims.GetUserEmail();

        return !string.IsNullOrWhiteSpace(user)
            ? user
            : _settings.ApplicationName;
    }
}
