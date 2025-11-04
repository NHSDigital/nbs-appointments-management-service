namespace Nhs.Appointments.Core;
public class SiteFilter
{
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public int SearchRadius { get; set; }
    public string OdsCode { get; set; }
    public string[] Types { get; set; }
    public AvailabilityFilter? Availability {  get; set; }
    public string[] AccessNeeds { get; set; }
    public int? Priority { get; set; }
}

public class AvailabilityFilter
{
    public string[] Services { get; set; }
    public DateOnly? From { get; set; }
    public DateOnly? Until { get; set; }
}
