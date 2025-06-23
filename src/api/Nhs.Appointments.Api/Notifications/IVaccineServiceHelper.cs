namespace Nhs.Appointments.Api.Notifications;
public interface IVaccineServiceHelper
{
    string GetVaccineType(string service);
    string GetServiceUrl(string service);
}
