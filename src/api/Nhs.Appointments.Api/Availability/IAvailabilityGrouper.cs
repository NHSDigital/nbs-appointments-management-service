using System.Collections.Generic;
using Nhs.Appointments.Api.Models;
using Nhs.Appointments.Core;

namespace Nhs.Appointments.Api.Availability;

public interface IAvailabilityGrouper
{
    IEnumerable<QueryAvailabilityResponseBlock> GroupAvailability(IEnumerable<SessionInstance> blocks);
}