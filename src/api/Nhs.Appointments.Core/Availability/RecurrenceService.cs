using Ical.Net;
using Ical.Net.CalendarComponents;
using Ical.Net.DataTypes;
using Ical.Net.Serialization;

namespace Nhs.Appointments.Core.Availability;

public interface IRecurrenceService
{
    IEnumerable<Occurrence> GenerateStandardOccurrences(DateOnly from, DateOnly until, Template template);

    IEnumerable<Occurrence> GenerateMovedOccurrences(DateOnly from, DateOnly until, Template template);
}

public class RecurrenceService : IRecurrenceService
{
    public IEnumerable<Occurrence> GenerateStandardOccurrences(DateOnly from, DateOnly until, Template template)
    {
        var session = template.Sessions.Single();
        var days = template.Days.Select(x => new WeekDay(x)).ToList();

        var startFromDatetime = new CalDateTime(from.Year, from.Month, from.Day, session.From.Hour, session.From.Minute,
            session.From.Second);
        var startUntilDatetime = new CalDateTime(from.Year, from.Month, from.Day, session.Until.Hour,
            session.Until.Minute,
            session.Until.Second);

        var end = new CalDateTime(until.Year, until.Month, until.Day, session.Until.Hour, session.Until.Minute,
            session.Until.Second);

        var recurrence = new RecurrencePattern
        {
            Frequency = FrequencyType.Weekly, ByDay = days, Interval = 1, Until = end
        };

        var calendarEvent = new CalendarEvent
        {
            DtStart = startFromDatetime, DtEnd = startUntilDatetime, RecurrenceRules = [recurrence], Sequence = 0
        };

        //TODO!!
        //are we going to have to store session times within recurrence rule???
        //that means EVERY slot that is exempt is going to be listed.......

        // calendarEvent.ExceptionDates.Add(new CalDateTime(2025, 07, 10, 22, 00, 00, "UTC"));

        // Calculate all occurrences
        return calendarEvent.GetOccurrences();
    }

    public IEnumerable<Occurrence> GenerateMovedOccurrences(DateOnly from, DateOnly until, Template template)
    {
        var start = new CalDateTime(2025, 07, 10, 09, 00, 00, "Europe/Zurich");
        var recurrence = new RecurrencePattern { Frequency = FrequencyType.Daily, Interval = 2, Count = 4 };

        var calendarEvent = new CalendarEvent
        {
            // UID links master with child.
            Uid = "my-custom-id",
            Summary = "Walking",
            DtStart = start,
            DtEnd = start.AddHours(1),
            RecurrenceRules = [recurrence],
            Sequence = 0 // default value
        };

        var startMoved = new CalDateTime(2025, 07, 14, 09, 00, 00, "Europe/Zurich");
        var movedEvent1 = new CalendarEvent
        {
            // UID links master with child.
            Uid = "my-custom-id",
            // Overwrite properties of the original occurrence.
            Summary = "Short after lunch walk",
            // Set new start and end time.
            DtStart = startMoved,
            DtEnd = startMoved.AddMinutes(15),
            // Set the original date of the occurrence (2025-07-14 09:00:00).
            RecurrenceIdentifier = new RecurrenceIdentifier(start.AddDays(4)),
            // The first change for this RecurrenceId
            Sequence = 1
        };
        
        var movedEvent2 = new CalendarEvent
        {
            // UID links master with child.
            Uid = "my-custom-id",
            // Overwrite properties of the original occurrence.
            Summary = "Short after lunch walk",
            // Set new start and end time.
            DtStart = startMoved.AddMinutes(45),
            DtEnd = startMoved.AddMinutes(60),
            // Set the original date of the occurrence (2025-07-14 09:00:00).
            RecurrenceIdentifier = new RecurrenceIdentifier(start.AddDays(4)),
            // The first change for this RecurrenceId
            Sequence = 2
        };
        
        //DO WE HAVE TO SPLIT INTO TWO??!!!?

        var calendar = new Calendar();
        calendar.Events.Add(calendarEvent);
        calendar.Events.Add(movedEvent1);
        calendar.Events.Add(movedEvent2);

        var calendarSerializer = new CalendarSerializer();
        var generatedIcs = calendarSerializer.SerializeToString(calendar);

        return calendar.GetOccurrences();
    }
}
