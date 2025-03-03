using static System.Decimal;

namespace Nhs.Appointments.Api.Models;

public sealed class SetSiteDetailsRequest
{
    public SetSiteDetailsRequest(
        string site,
        string name,
        string address,
        string phoneNumber,
        string latitude,
        string longitude)
    {
        Site = site;
        Name = name;
        Address = address;
        PhoneNumber = phoneNumber;
        Latitude = latitude;
        Longitude = longitude;

        TryParse(Latitude, out var latDecimal);
        LatitudeDecimal = latDecimal;
        TryParse(Longitude, out var longDecimal);
        LongitudeDecimal = longDecimal;
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
