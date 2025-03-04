using System.Collections.Generic;
using System.Threading.Tasks;

namespace Nhs.Appointments.Api;

using Microsoft.FeatureManagement.FeatureFilters;

/// <summary>
/// Use SiteRequestInspector??
/// </summary>
public class TargetingContextAccessor : ITargetingContextAccessor
{
    public ValueTask<TargetingContext> GetContextAsync()
    {
        var context = new TargetingContext
        {
            UserId = "user1@example.com", //replace with API context user
            Groups = new List<string> { "BetaTesters", "GeneralUsers" }
        };
    
        return new ValueTask<TargetingContext>(context);
    }
}
