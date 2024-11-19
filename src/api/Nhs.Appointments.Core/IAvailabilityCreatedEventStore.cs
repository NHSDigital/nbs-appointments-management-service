namespace Nhs.Appointments.Core;

public interface IAvailabilityCreatedEventStore
{
    Task LogTemplateCreated(string site, DateOnly from, DateOnly until, Template template, string user);
    Task LogSingleDateSessionCreated(string site, DateOnly date, Session[] sessions, string user);
}