namespace Nhs.Appointments.Core;
public class SiteFilter
{
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public int SearchRadius { get; set; }
    public string OdsCode { get; set; }
    public string[] Types { get; set; }
    public string[] Services { get; set; }
    public DateOnly? From {  get; set; }
    public DateOnly? To { get; set; }
    public string[] AccessNeeds { get; set; }
}
