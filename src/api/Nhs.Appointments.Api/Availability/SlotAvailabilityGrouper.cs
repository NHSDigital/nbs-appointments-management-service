using System;
using System.Collections.Generic;
using System.Linq;
using Nhs.Appointments.Core;

namespace Nhs.Appointments.Api.Availability;

public class SlotAvailabilityGrouper : IAvailabilityGrouper
{
    public IEnumerable<QueryAvailabilityResponseBlock> GroupAvailability(IEnumerable<SessionInstance> blocks)
    {
        if (blocks == null) throw new ArgumentNullException(nameof(blocks));
        
        return blocks
            .GroupBy(x => (x.From, x.Duration)) // Need to group by from and duration
            .Select(dataItem => new QueryAvailabilityResponseBlock(TimeOnly.FromDateTime(dataItem.Key.From), TimeOnly.FromDateTime(dataItem.Key.From.Add(dataItem.Key.Duration)), dataItem.Sum(x => x.Capacity)))
            .ToList();
    }
}