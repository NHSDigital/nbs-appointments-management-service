using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.AspNetCore.Authorization;
using Nhs.Appointments.Core.Bookings;
using Nhs.Appointments.Core;
using System;

namespace Nhs.Appointments.Api.Functions.TimerFunctions;

public class ScheduledUnconfirmedProvisionalBookingsCollectorFunction(IBookingWriteService bookingWriteService)
{
    [Function("RemoveUnconfirmedProvisionalBookings")]
    [AllowAnonymous]
    public Task RemoveUnconfirmedProvisionalBookings([TimerTrigger("%UnconfirmedProvisionalBookingsCronSchedule%")] TimerInfo timerInfo)
    {
        return bookingWriteService.RemoveUnconfirmedProvisionalBookings();
    }
}

