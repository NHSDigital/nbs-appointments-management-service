namespace Nhs.Appointments.Api.Auth;

public interface IRequestAuthenticatorFactory
{
    IRequestAuthenticator CreateAuthenticator(string scheme);
}
