using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.AspNetCore.Authorization;
using Nhs.Appointments.Core;
using Nhs.Appointments.Core.Reports.SiteSummary;

namespace Nhs.Appointments.Api.Functions;

public class ScheduledDailySiteSummaryAggregationFunction(ISitesSummaryTrigger sitesSummaryTrigger)
{
    [Function("DailySiteSummaryAggregation")]
    [AllowAnonymous]
    public Task SendBookingRemindersAsync([TimerTrigger("%DailySiteSummaryAggregationCronSchedule%")] TimerInfo timerInfo)
    {
        return sitesSummaryTrigger.Trigger();
    }     
}

