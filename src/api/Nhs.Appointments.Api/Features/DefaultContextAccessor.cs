using System.Threading.Tasks;
using Microsoft.FeatureManagement.FeatureFilters;

namespace Nhs.Appointments.Api.Features;

public class DefaultContextAccessor : ITargetingContextAccessor
{
    public async ValueTask<TargetingContext> GetContextAsync()
    {
        //no default global context, it must be supplied
        return await new ValueTask<TargetingContext>(result: null);
    }
}
