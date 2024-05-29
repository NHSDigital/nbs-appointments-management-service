using Nhs.Appointments.Core;

namespace Nhs.Appointments.Api.Tests;

public static class AvailabilityHelper
{
    public static List<SessionInstance> CreateTestBlocks(params string[] timesAndHolders)
    {
        var blocks = new List<SessionInstance>();
        foreach (var item in timesAndHolders)
        {
            var sessionSplit = item.Split("|");
            var sessionHolder = sessionSplit.Length > 1 ? sessionSplit[1] : "SessionHolder";
            var limits= sessionSplit[0].Split("-");
            var from = TimeSpan.ParseExact(limits[0], @"hh\:mm", null);
            var until = TimeSpan.ParseExact(limits[1], @"hh\:mm", null);
            DateTime date = new DateTime(2077, 1, 1);
            
            blocks.Add(new SessionInstance(date.Add(from), date.Add(until))
            {
                SessionHolder = sessionHolder
            });
        }
        return blocks;
    }
}
