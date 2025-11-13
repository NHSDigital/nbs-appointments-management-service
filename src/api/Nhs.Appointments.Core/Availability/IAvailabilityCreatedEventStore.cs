namespace Nhs.Appointments.Core.Availability;

public interface IAvailabilityCreatedEventStore
{
    Task LogTemplateCreated(string site, DateOnly from, DateOnly until, Template template, string user);
    Task LogSingleDateSessionCreated(string site, DateOnly date, Session[] sessions, string user);
    Task<IEnumerable<AvailabilityCreatedEvent>> GetAvailabilityCreatedEvents(string site);
}