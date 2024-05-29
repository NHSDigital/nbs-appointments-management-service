namespace Nhs.Appointments.Api.Auth;

public class AuthOptions
{
    public string ProviderUri { get; set; }
    public string TokenPath { get; set; }
    public string AuthorizePath { get; set; }
    public string JwksPath { get; set; }
    public string Issuer { get; set; }        
    public string ClientId { get; set; }
    public string ReturnUri { get; set; }   
    public string ChallengePhrase { get; set; }
}
