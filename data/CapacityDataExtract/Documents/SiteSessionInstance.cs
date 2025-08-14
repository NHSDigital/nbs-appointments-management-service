using Nhs.Appointments.Core;
using Nhs.Appointments.Persistance.Models;

namespace CapacityDataExtracts.Documents;
public class SiteSessionInstance : SessionInstance
{
    public SiteSessionInstance(SiteDocument site, TimePeriod timePeriod) : base(timePeriod.From, timePeriod.Until) 
    {
        Site = site;
    }
    public SiteSessionInstance(SiteDocument site, DateTime from, DateTime until) : base(from, until)
    {
        Site = site;
    }

    public SiteDocument Site { get; set; }

    public IEnumerable<SiteSessionInstance> ToSiteSlots() => Divide(TimeSpan.FromMinutes(SlotLength)).Select(sl =>
            new SiteSessionInstance(Site, sl) { Services = Services, Capacity = Capacity });
}
