using System;
using System.Collections.Generic;
using System.Linq;
using Nhs.Appointments.Core;

namespace Nhs.Appointments.Api.Availability;

public class HourlyAvailabilityGrouper : IAvailabilityGrouper
{
    public IEnumerable<QueryAvailabilityResponseBlock> GroupAvailability(IEnumerable<SessionInstance> slots)
    {
        if (slots == null) throw new ArgumentNullException(nameof(slots));                       

        return slots            
            .GroupBy(sl => sl.From.Hour)
            .Select(dataItem => new QueryAvailabilityResponseBlock(new TimeOnly(dataItem.Key, 0), new TimeOnly(dataItem.Key + 1, 0), dataItem.Sum(i => i.Capacity)))
            .ToList();        
    }
}