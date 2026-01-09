using Nhs.Appointments.Core.Users;
using Nhs.Appointments.Persistance;
using System.Security.Claims;

namespace Nhs.Appointments.Api.Providers;

internal class LastUpdatedByResolver(IUserContextProvider userContextProvider) : ILastUpdatedByResolver
{
    public string GetLastUpdatedBy(){
        var user = userContextProvider.UserPrincipal?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return user;
    }
}
