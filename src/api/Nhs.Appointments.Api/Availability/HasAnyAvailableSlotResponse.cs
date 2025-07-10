using System.Collections.Generic;

namespace Nhs.Appointments.Api.Availability;

public class HasAnyAvailableSlotResponse : List<HasAnyAvailableSlotResponseItem>
{
}

public record HasAnyAvailableSlotResponseItem(string site, bool hasAnyAvailableSlot);
