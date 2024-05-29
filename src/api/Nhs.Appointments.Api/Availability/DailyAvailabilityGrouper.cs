using System;
using System.Collections.Generic;
using System.Linq;
using Nhs.Appointments.Core;

namespace Nhs.Appointments.Api.Availability;

public class DailyAvailabilityGrouper : IAvailabilityGrouper
{
    public IEnumerable<QueryAvailabilityResponseBlock> GroupAvailability(IEnumerable<TimePeriod> blocks, int slotDuration)
    {
        if (blocks == null) throw new ArgumentNullException(nameof(blocks));
        if (slotDuration == 0) throw new ArgumentOutOfRangeException(nameof(slotDuration));

        var amCount = 0;
        var pmCount = 0;

        if (blocks.Any())
        {
            var amPmLine = blocks.First().From.Date.AddHours(12);
            var splitBlocks = blocks.SelectMany(b => b.Split(amPmLine));
            amCount = splitBlocks.Where(b => b.From.Hour < 12).Sum(b => (int)b.Duration.TotalMinutes / slotDuration);
            pmCount = splitBlocks.Where(b => b.From.Hour >= 12).Sum(b => (int)b.Duration.TotalMinutes / slotDuration);
        }
        return new List<QueryAvailabilityResponseBlock>
        {
            new (new TimeOnly(0, 0), new TimeOnly(11, 59), amCount),
            new (new TimeOnly(12, 0), new TimeOnly(23, 59), pmCount),
        };
    }
}