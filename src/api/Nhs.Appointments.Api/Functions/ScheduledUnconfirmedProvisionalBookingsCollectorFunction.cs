using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.AspNetCore.Authorization;
using Nhs.Appointments.Core;

namespace Nhs.Appointments.Api.Functions;

public class ScheduledUnconfirmedProvisionalBookingsCollectorFunction(IBookingWriteService bookingWriteService)
{
    [Function("RemoveUnconfirmedProvisionalBookings")]
    [AllowAnonymous]
    public Task RemoveUnconfirmedProvisionalBookings([TimerTrigger("%UnconfirmedProvisionalBookingsCronSchedule%")] TimerInfo timerInfo)
    {
        return bookingWriteService.RemoveUnconfirmedProvisionalBookings();
    }
}

