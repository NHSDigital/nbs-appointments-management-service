namespace Nhs.Appointments.Core;

public interface IAvailabilityService
{
    Task ApplyAvailabilityTemplateAsync(string site, DateOnly from, DateOnly until, Template template);
    Task SetAvailabilityAsync(DateOnly date, string site, Session[] sessions);
}
