namespace Nhs.Appointments.Core;

public interface IAvailabilityWriteService
{
    Task ApplyAvailabilityTemplateAsync(string site, DateOnly from, DateOnly until, Template template, ApplyAvailabilityMode mode, string user);

    Task ApplySingleDateSessionAsync(DateOnly date, string site, Session[] sessions, ApplyAvailabilityMode mode,
        string user, Session sessionToEdit = null);
    Task CancelSession(string site, DateOnly date, string from, string until, string[] services, int slotLength, int capacity);
}
