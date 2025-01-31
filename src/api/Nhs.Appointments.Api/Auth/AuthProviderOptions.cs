namespace Nhs.Appointments.Api.Auth;

public class AuthProviderOptions
{
    public string Name { get; set; }
    public string TokenUri { get; set; }
    public string AuthorizeUri { get; set; }
    public string JwksUri { get; set; }
    public string Issuer { get; set; }        
    public string ClientId { get; set; }
    public string ReturnUri { get; set; }   
    public string ChallengePhrase { get; set; }
    public string ClientCodeExchangeUri { get; set; }
    public bool RequiresStateForAuthorize { get; set; }
    public string ClientSecret { get; set; }
}
