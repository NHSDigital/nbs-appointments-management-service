using System;
using Nhs.Appointments.Api.Availability;

namespace Nhs.Appointments.Api;

public class AvailabilityGrouperFactory : IAvailabilityGrouperFactory
{
    public IAvailabilityGrouper Create(QueryType queryType) => queryType switch
    {
        QueryType.Days => new DailyAvailabilityGrouper(),
        QueryType.Hours => new HourlyAvailabilityGrouper(),
        QueryType.Slots => new SlotAvailabilityGrouper(),
        _ => throw new NotSupportedException($"{queryType} is not a valid queryType")
    };
}