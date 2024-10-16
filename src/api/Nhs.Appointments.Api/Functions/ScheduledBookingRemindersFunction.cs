using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.AspNetCore.Authorization;
using Nhs.Appointments.Core;

namespace Nhs.Appointments.Api.Functions;

public class ScheduledBookingRemindersFunction(IBookingsService bookingService)
{
    private const string EveryMorningAtEight = "0 0 8 * * *";
    [Function("SendBookingReminders")]
    [AllowAnonymous]
    public Task SendBookingRemindersAsync([TimerTrigger(EveryMorningAtEight)] TimerInfo timerInfo)
    {
        return bookingService.SendBookingReminders();

    }     
}


