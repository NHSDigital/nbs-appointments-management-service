using System;
using System.Collections.Generic;
using System.Linq;
using Nhs.Appointments.Api.Models;
using Nhs.Appointments.Core;

namespace Nhs.Appointments.Api.Availability;

public class HourlyAvailabilityGrouper : IAvailabilityGrouper
{
    public IEnumerable<QueryAvailabilityResponseBlock> GroupAvailability(IEnumerable<TimePeriod> blocks, int slotDuration)
    {
        if (blocks == null) throw new ArgumentNullException(nameof(blocks));
        if (slotDuration == 0) throw new ArgumentOutOfRangeException(nameof(slotDuration));
        
        return blocks
            .SelectMany(b => b.Divide(TimeSpan.FromMinutes(slotDuration)))
            .GroupBy(sl => sl.From.Hour)
            .Select(dataItem => new QueryAvailabilityResponseBlock(new TimeOnly(dataItem.Key, 0), new TimeOnly(dataItem.Key + 1, 0), dataItem.Count()))
            .ToList();        
    }
}