using Nhs.Appointments.Core;

namespace CapacityDataExtract.Documents;
public class SiteSessionInstance : SessionInstance
{
    public SiteSessionInstance(string site, TimePeriod timePeriod) : base(timePeriod.From, timePeriod.Until) 
    {
        Site = site;
    }
    public SiteSessionInstance(string site, DateTime from, DateTime until) : base(from, until)
    {
        Site = site;
    }

    public string Site { get; set; }

    public IEnumerable<SiteSessionInstance> ToSiteSlots() => Divide(TimeSpan.FromMinutes(SlotLength)).Select(sl =>
            new SiteSessionInstance(Site, sl) { Services = Services, Capacity = Capacity });
}
