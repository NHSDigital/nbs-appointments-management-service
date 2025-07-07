using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.AspNetCore.Authorization;
using Nhs.Appointments.Core;

namespace Nhs.Appointments.Api.Functions;

public class ScheduledBookingRemindersFunction(IBookingWriteService bookingWriteService)
{
    [Function("SendBookingReminders")]
    [AllowAnonymous]
    public Task SendBookingRemindersAsync([TimerTrigger("%BookingRemindersCronSchedule%")] TimerInfo timerInfo)
    {
        return bookingWriteService.SendBookingReminders();
    }     
}

