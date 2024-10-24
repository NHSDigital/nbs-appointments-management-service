using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.AspNetCore.Authorization;
using Nhs.Appointments.Core;

namespace Nhs.Appointments.Api.Functions;

public class ScheduledUnconfirmedProvisionalBookingsCollectorFunction(IBookingsService bookingsService)
{
    [Function("RemoveUnconfirmedProvisionalBookings")]
    [AllowAnonymous]
    public Task RemoveUnconfirmedProvisionalBookings([TimerTrigger("%UnconfirmedProvisionalBookingsCronSchedule%")] TimerInfo timerInfo)
    {
        return bookingsService.RemoveUnconfirmedProvisionalBookings();
    }
}

