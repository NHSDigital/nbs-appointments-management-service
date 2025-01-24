namespace Nhs.Appointments.Api.Models;

public record SetSiteDetailsRequest(
    string Site,
    string Name,
    string Address,
    string PhoneNumber,
    string Latitude,
    string Longitude)
{
    public decimal LatitudeDecimal => decimal.Parse(Latitude);
    public decimal LongitudeDecimal => decimal.Parse(Longitude);
};
