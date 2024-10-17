using System;
using System.Collections.Generic;
using System.Linq;
using Nhs.Appointments.Core;

namespace Nhs.Appointments.Api.Availability;

public class DailyAvailabilityGrouper : IAvailabilityGrouper
{
    public IEnumerable<QueryAvailabilityResponseBlock> GroupAvailability(IEnumerable<SessionInstance> slots)
    {
        if (slots == null) throw new ArgumentNullException(nameof(slots));        

        var amCount = 0;
        var pmCount = 0;

        if (slots.Any())
        {
            amCount = slots.Where(b => b.From.Hour < 12).Sum(s => s.Capacity);
            pmCount = slots.Where(b => b.From.Hour >= 12).Sum(s => s.Capacity);
        }
        return new List<QueryAvailabilityResponseBlock>
        {
            new (new TimeOnly(0, 0), new TimeOnly(11, 59), amCount),
            new (new TimeOnly(12, 0), new TimeOnly(23, 59), pmCount),
        };
    }
}