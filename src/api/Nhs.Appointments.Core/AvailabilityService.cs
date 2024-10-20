namespace Nhs.Appointments.Core;

public class AvailabilityService(IAvailabilityStore availabilityStore) : IAvailabilityService
{
    public async Task ApplyTemplateAsync(string site, DateOnly from, DateOnly until, Template template)
    {
        if (string.IsNullOrEmpty(site))
            throw new ArgumentException("site must have a value");

        if (from > until)
            throw new ArgumentException("until date must be after from date");

        if (template == null)
            throw new ArgumentException("template must be provided");

        if (template.Sessions?.Length == 0)
            throw new ArgumentException("template must contain one or more sessions");

        if (template.Days?.Length == 0)
            throw new ArgumentException("template must specify one or more weekdays");
        
        var dates = GetDatesBetween(from, until, template.Days);
        foreach (var date in dates)
        {
            await availabilityStore.ApplyTemplate(site, date, template.Sessions);
        }
    }

    private static IEnumerable<DateOnly> GetDatesBetween(DateOnly start, DateOnly end, params DayOfWeek[] weekdays)
    {
        var cursor = start;
        while (cursor <= end)
        {
            if (weekdays.Contains(cursor.DayOfWeek))
                yield return cursor;
            cursor = cursor.AddDays(1);
        }
    }
}
