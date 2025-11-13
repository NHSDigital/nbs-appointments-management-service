using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Nhs.Appointments.Core.Availability;

namespace Nhs.Appointments.Api.Availability;

public class HourlyAvailabilityGrouper : IAvailabilityGrouper
{
    public IEnumerable<QueryAvailabilityResponseBlock> GroupAvailability(IEnumerable<SessionInstance> slots)
    {
        if (slots == null) throw new ArgumentNullException(nameof(slots));

        return slots
            .GroupBy(sl => sl.From.Hour)
            .Select(dataItem => new QueryAvailabilityResponseBlock(
                new TimeOnly(dataItem.Key, 0),
                dataItem.Key == 23
                    ? new TimeOnly(0, 0)
                    : new TimeOnly(dataItem.Key + 1, 0), // if its 23 then set midnight as 24 errors
                dataItem.Sum(i => i.Capacity)))
            .OrderBy(x => x.from)
            .ToList();
    }
}
