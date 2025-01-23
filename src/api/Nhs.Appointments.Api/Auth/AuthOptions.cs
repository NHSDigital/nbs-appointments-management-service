using System.Collections.Generic;

namespace Nhs.Appointments.Api.Auth;

public class AuthOptions
{
    public List<AuthProviderOptions> Providers { get; set; }
}
