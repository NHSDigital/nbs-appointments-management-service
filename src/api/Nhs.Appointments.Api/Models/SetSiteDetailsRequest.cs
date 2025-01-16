namespace Nhs.Appointments.Api.Models;

public record SetSiteDetailsRequest(string Site, string Name, string Address, string PhoneNumber, string Latitude, string Longitude);
