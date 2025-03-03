using static System.Decimal;

namespace Nhs.Appointments.Api.Models;

public sealed class SetSiteDetailsRequest
{
    public SetSiteDetailsRequest(
        string site,
        string name,
        string address,
        string phoneNumber,
        string longitude,
        string latitude)
    {
        Site = site;
        Name = name;
        Address = address;
        PhoneNumber = phoneNumber;

        TryParse(Longitude, out var longDecimal);
        LongitudeDecimal = longDecimal;
        Longitude = longitude;

        TryParse(Latitude, out var latDecimal);
        LatitudeDecimal = latDecimal;
        Latitude = latitude;
    }

    public string Site { get; }
    public string Name { get; }
    public string Address { get; }
    public string PhoneNumber { get; }

    public decimal LatitudeDecimal { get; }
    public string Latitude { get; }

    public decimal LongitudeDecimal { get; }
    public string Longitude { get; }
}
