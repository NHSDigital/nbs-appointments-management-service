namespace Nhs.Appointments.Api.Models;

public record GetSitesByAreaRequest(double longitude, double latitude, int searchRadius, int maximumRecords);
