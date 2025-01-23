using System.Security.Claims;

namespace Nhs.Appointments.Core;

public interface IUserContextProvider
{
    ClaimsPrincipal UserPrincipal { get; }
}

