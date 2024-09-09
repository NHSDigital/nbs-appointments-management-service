namespace Nhs.Appointments.Api.Models;

public record GetSitesRequest(double longitude, double latitude, int searchRadius, int maximumRecords);
