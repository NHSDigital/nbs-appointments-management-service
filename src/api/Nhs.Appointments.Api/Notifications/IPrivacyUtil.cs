namespace Nhs.Appointments.Api.Notifications
{
    public interface IPrivacyUtil
    {
        string ObfuscateEmail(string email);
        string ObfuscatePhoneNumber(string phoneNumber);
    }
}