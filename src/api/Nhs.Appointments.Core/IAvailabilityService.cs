namespace Nhs.Appointments.Core;

public interface IAvailabilityService
{
    Task ApplyAvailabilityTemplateAsync(string site, DateOnly from, DateOnly until, Template template, ApplyAvailabilityMode mode, string user);
    Task ApplySingleDateSessionAsync(DateOnly date, string site, Session[] sessions, ApplyAvailabilityMode mode, string user);
    Task<IEnumerable<AvailabilityCreatedEvent>> GetAvailabilityCreatedEventsAsync(string site, DateOnly from);
}
