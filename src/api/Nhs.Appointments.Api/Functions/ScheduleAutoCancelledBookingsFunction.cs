using Microsoft.AspNetCore.Authorization;
using Microsoft.Azure.Functions.Worker;
using Nhs.Appointments.Core;
using System.Threading.Tasks;

namespace Nhs.Appointments.Api.Functions;
public class ScheduleAutoCancelledBookingsFunction(IBookingWriteService bookingWriteService)
{
    [AllowAnonymous]
    [Function("SendAutoCancelledBookings")]
    public async Task ScheduleAutoCancelledBookingsAsync([TimerTrigger("%AutoCancelledBookingsCronSchedule%")] TimerInfo timer)
    {
        await bookingWriteService.SendAutoCancelledBookingNotifications();
    }
}
