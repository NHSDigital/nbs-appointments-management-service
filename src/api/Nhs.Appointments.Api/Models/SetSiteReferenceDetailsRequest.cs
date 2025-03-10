namespace Nhs.Appointments.Api.Models;

public record SetSiteReferenceDetailsRequest(string Site, string OdsCode, string Icb, string Region);
