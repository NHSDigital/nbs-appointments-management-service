using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.AspNetCore.Authorization;
using Nhs.Appointments.Core;
using System;

namespace Nhs.Appointments.Api.Functions;

public class ScheduledUnconfirmedProvisionalBookingsCollectorFunction(IBookingWriteService bookingWriteService)
{
    [Function("RemoveUnconfirmedProvisionalBookings")]
    [AllowAnonymous]
    public Task RemoveUnconfirmedProvisionalBookings([TimerTrigger("%UnconfirmedProvisionalBookingsCronSchedule%")] TimerInfo timerInfo)
    {
        var batchSize = int.Parse(Environment.GetEnvironmentVariable("CleanupBatchSize") ?? "200");

        var degreeOfParallelism = int.Parse(Environment.GetEnvironmentVariable("CleanupDegreeOfParallelism") ?? "8");

        return bookingWriteService.RemoveUnconfirmedProvisionalBookings(batchSize, degreeOfParallelism);
    }
}

