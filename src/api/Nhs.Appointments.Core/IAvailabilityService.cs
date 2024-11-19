namespace Nhs.Appointments.Core;

public interface IAvailabilityService
{
    Task ApplyAvailabilityTemplateAsync(string site, DateOnly from, DateOnly until, Template template, string user);
    Task ApplySingleDateSessionAsync(DateOnly date, string site, Session[] sessions, string user);
}
