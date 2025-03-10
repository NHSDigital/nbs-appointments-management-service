using Nbs.Appointments.Calculator.Models;

namespace Nbs.Appointments.Calculator;
public class AvailibilityCalculator : IAvailibilityCalculator
{
    public AvailibilityCalculator(IRelatedAvailabilityResolver relatedAvailabilityResolver)
    {
        RelatedAvailabilityResolver = relatedAvailabilityResolver;
    }

    private IRelatedAvailabilityResolver RelatedAvailabilityResolver { get; }

    public Dictionary<string, int> GetCapacity(IEnumerable<Availability> availabilities, string[] bookedServices) 
    {
        var report = GetCapacityReport(availabilities, bookedServices);

        return report.Available;
    }

    public int GetCapacityForService(string service, IEnumerable<Availability> availabilities, string[] bookedServices) 
    {
        var availibility = GetCapacity(availabilities, bookedServices);

        if (!availibility.TryGetValue(service, out var capacity)) 
        {
            return 0;
        }

        return capacity;
    }

    public Capacity GetCapacityReport(IEnumerable<Availability> availabilities, string[] bookedServices)
    {
        var relatedServices = bookedServices.Union(availabilities.SelectMany(x => x.Services)).Distinct()
            .ToDictionary(
            x => x, 
            x => RelatedAvailabilityResolver.ResolveRelatedAvailabilities([x], availabilities).ToArray());

        bookedServices = bookedServices.OrderByDescending(x => bookedServices.Count(bs => bs.Equals(x))).ToArray();

        foreach (var availibility in availabilities.OrderBy(av => av.Services.Count()).ThenBy(av => bookedServices.Count(bs => av.Services.Contains(bs)))) 
        {
            bookedServices = availibility.TryAddBookings(bookedServices);
        }

        return new Capacity() 
        {
            Available = relatedServices.ToDictionary(
                x => x.Key,
                x => Math.Max(0, availabilities.Where(av => av.Services.Any(service => x.Value.Contains(service))).Sum(av => av.RemainingCapacity) - bookedServices.Count(bs => bs.Equals(x.Key)))),
            NotAvailable = bookedServices.Distinct().Select(x => 
            new UnAvailable(
                RelatedAvailabilityResolver.ResolveRelatedAvailabilities([x], availabilities).ToArray(), 
                bookedServices.Count(bs => bs.Equals(x))))
        };
    }

    public IEnumerable<UnAvailable> GetNotAvailableCapacity(IEnumerable<Availability> availabilities, string[] bookedServices) 
    {
        var report = GetCapacityReport(availabilities, bookedServices);

        return report.NotAvailable;
    }
}
