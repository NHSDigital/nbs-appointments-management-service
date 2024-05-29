using System;
using System.Collections.Generic;
using System.Linq;
using Nhs.Appointments.Core;

namespace Nhs.Appointments.Api.Availability;

public class SlotAvailabilityGrouper : IAvailabilityGrouper
{
    public IEnumerable<QueryAvailabilityResponseBlock> GroupAvailability(IEnumerable<TimePeriod> blocks, int slotDuration)
    {
        if (blocks == null) throw new ArgumentNullException(nameof(blocks));
        if (slotDuration == 0) throw new ArgumentOutOfRangeException(nameof(slotDuration));
        
        return blocks
            .SelectMany(b => b.Divide(TimeSpan.FromMinutes(slotDuration)))
            .GroupBy(x => x.From)
            .Select(dataItem => new QueryAvailabilityResponseBlock(TimeOnly.FromDateTime(dataItem.Key), TimeOnly.FromDateTime(dataItem.Key.AddMinutes(slotDuration)), dataItem.Count()))
            .ToList();
    }
}