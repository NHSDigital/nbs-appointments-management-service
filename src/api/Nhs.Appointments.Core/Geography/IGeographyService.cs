namespace Nhs.Appointments.Core.Geography;

public interface IGeographyService
{
    int CalculateDistanceInMetres(Coordinates origin, Coordinates destination);
}
