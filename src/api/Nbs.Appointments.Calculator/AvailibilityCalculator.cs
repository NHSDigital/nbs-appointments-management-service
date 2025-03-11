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
        var distinctServices = bookedServices.Union(availabilities.SelectMany(x => x.Services)).Distinct();
        var reports = new List<Capacity>();
        var final = new Capacity();

        foreach (var service in distinctServices) 
        {
            var serviceReport = new Capacity();//GetCapacityReportForService(service, availabilities, bookedServices);
            reports.Add(serviceReport);
            final.Available.Add(service, serviceReport.Available[service]);
        }

        return final;
    }

    //private Capacity GetCapacityReportForService(string service, IEnumerable<Availability> availabilities, string[] bookedServices) 
    //{
    //    var report = new Capacity();
    //    if (!bookedServices.Any())
    //    {
    //        report.Available.Add(service, availabilities.Where(x => x.Services.Contains(service)).Sum(x => x.Capacity));
    //        return report;
    //    }

    //    foreach (var presentService in bookedServices.Union(availabilities.SelectMany(x => x.Services)).Distinct()
    //        .OrderBy(x => x.Equals(service) ? 1 : 0)
    //        .ThenByDescending(x => bookedServices.Count(bs => x.Contains(bs))))
    //    {
    //        var count = bookedServices.Count(x => x.Equals(presentService));
    //        availabilities.OrderBy(x => x.Services.Contains(presentService) ? 1 : 0)
    //                .ThenByDescending(x => bookedServices.Count(bs => x.Services.Contains(bs)));

    //        while (count > 0)
    //        {

    //        }
    //    }
    //}

    //public Capacity GetCapacityReport(IEnumerable<Availability> availabilities, string[] bookedServices) 
    //{
    //    var distinctServices = bookedServices.Union(availabilities.SelectMany(x => x.Services)).Distinct();
    //    var tree = availabilities.Select(x => new LinkedAvailability(x, availabilities.Where(av => !av.Id.Equals(x.Id) && av.Services.Any(service => x.Services.Contains(service)))));
    //    var report = new Capacity();

    //    foreach (var service in distinctServices) 
    //    {
    //        var serviceBranches = tree.Where(node => node.Availability.Services.Contains(service));

    //        Console.WriteLine($"Branches for {service}");
    //        foreach (var serviceBranch in serviceBranches) 
    //        {
    //            serviceBranch.PrintToConsole();
    //        }

    //        var calcuated = serviceBranches.Select(
    //            x => x.TryBookService(service, bookedServices)).ToList();

    //        report.Available.Add(service, calcuated.Sum(x => x.RemainingCapacity));
    //        report.NotAvailable.Add(new UnAvailable([service], calcuated.Min(x => x.RemainingServices.Count(rs => rs.Equals(service)))));

    //    }

    //    return report;
    //}

    //public Capacity GetCapacityReport(IEnumerable<Availability> availabilities, string[] bookedServices)
    //{
    //    var relatedServices = bookedServices.Union(availabilities.SelectMany(x => x.Services)).Distinct()
    //        .ToDictionary(
    //        x => x, 
    //        x => RelatedAvailabilityResolver.ResolveRelatedAvailabilities([x], availabilities).ToArray());

    //    bookedServices = bookedServices.OrderByDescending(x => bookedServices.Count(bs => bs.Equals(x))).ToArray();

    //    foreach (var availibility in availabilities.OrderBy(av => av.Services.Count()).ThenBy(av => bookedServices.Count(bs => av.Services.Contains(bs)))) 
    //    {
    //        bookedServices = availibility.TryAddBookings(bookedServices);
    //    }

    //    return new Capacity() 
    //    {
    //        Available = relatedServices.ToDictionary(
    //            x => x.Key,
    //            x => Math.Max(0, availabilities.Where(av => av.Services.Any(service => x.Value.Contains(service))).Sum(av => av.RemainingCapacity) - bookedServices.Count(bs => bs.Equals(x.Key)))),
    //        NotAvailable = bookedServices.Distinct().Select(x => 
    //        new UnAvailable(
    //            RelatedAvailabilityResolver.ResolveRelatedAvailabilities([x], availabilities).ToArray(), 
    //            bookedServices.Count(bs => bs.Equals(x))))
    //    };
    //}

    public IEnumerable<UnAvailable> GetNotAvailableCapacity(IEnumerable<Availability> availabilities, string[] bookedServices) 
    {
        var report = GetCapacityReport(availabilities, bookedServices);

        return report.NotAvailable;
    }
}
