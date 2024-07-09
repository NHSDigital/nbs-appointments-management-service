using System.Security.Claims;

namespace Nhs.Appointments.Api.Tests;

public static class UserDataGenerator
{
    public static ClaimsPrincipal CreateUserPrincipal(string emailAddress)
    {
        var claims = new List<Claim>()
        {
            new Claim(ClaimTypes.Email, emailAddress),                
        };
        var identity = new ClaimsIdentity(claims, "TestAuthType");
        return new ClaimsPrincipal(identity);
    }
}
