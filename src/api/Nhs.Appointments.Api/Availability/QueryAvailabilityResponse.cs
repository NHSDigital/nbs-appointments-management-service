using System;
using System.Collections.Generic;

namespace Nhs.Appointments.Api.Availability;

public class QueryAvailabilityResponse : List<QueryAvailabilityResponseItem> { }
public record QueryAvailabilityResponseItem(string site, string service, List<QueryAvailabilityResponseInfo> availability);

public record QueryAvailabilityResponseInfo(DateOnly date, IEnumerable<QueryAvailabilityResponseBlock> blocks);

public record QueryAvailabilityResponseBlock(TimeOnly from, TimeOnly until, int count);
