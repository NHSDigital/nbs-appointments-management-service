namespace Nhs.Appointments.Core;

public interface IAvailabilityService
{
    Task ApplyTemplateAsync(string site, DateOnly from, DateOnly until, Template template);
}
