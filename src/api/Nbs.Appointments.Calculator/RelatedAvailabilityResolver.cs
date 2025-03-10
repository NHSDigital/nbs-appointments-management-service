using Nbs.Appointments.Calculator.Models;

namespace Nbs.Appointments.Calculator;
public class RelatedAvailabilityResolver : IRelatedAvailabilityResolver
{
    public IEnumerable<string> ResolveRelatedAvailabilities(string[] services, IEnumerable<Availability> availabilities) 
    {
        var related = availabilities.Where(av => av.Services.Any(service => services.Contains(service)));
        var distinctServices = related.SelectMany(r => r.Services).Union(services).Distinct().ToArray();

        if (distinctServices.Count() > services.Count())
        {
            return ResolveRelatedAvailabilities(distinctServices, availabilities);
        }

        return distinctServices;
    }
}
