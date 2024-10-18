using Nhs.Appointments.Core;

namespace Nhs.Appointments.Api.Tests;

public static class AvailabilityHelper
{
    public static List<SessionInstance> CreateTestSlots(DateOnly date, TimeOnly from, TimeOnly until, TimeSpan slotLength, int capacity = 1)
    {
        var slots = new List<SessionInstance>();
        var cursor = from;

        while(cursor < until)
        {
            slots.Add(new SessionInstance(date.ToDateTime(cursor), date.ToDateTime(cursor.Add(slotLength))) { Capacity = capacity });
            cursor = cursor.Add(slotLength);
        }
               
        return slots;
    }
}
