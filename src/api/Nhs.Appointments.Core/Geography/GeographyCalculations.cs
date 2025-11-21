namespace Nhs.Appointments.Core.Geography;

public static class GeographyCalculations
{
    public static int CalculateDistanceInMetres(Coordinates origin, Coordinates destination)
    {
        var epsilon = 0.000001f;
        var deltaLatitude = destination.Latitude - origin.Latitude;
        var deltaLongitude = destination.Longitude - origin.Longitude;

        if (Math.Abs(deltaLatitude) < epsilon && Math.Abs(deltaLongitude) < epsilon)
        {
            return 0;
        }

        var dist = (Math.Sin(DegreesToRadians(destination.Latitude)) * Math.Sin(DegreesToRadians(origin.Latitude))) +
                   (Math.Cos(DegreesToRadians(destination.Latitude)) * Math.Cos(DegreesToRadians(origin.Latitude)) *
                    Math.Cos(DegreesToRadians(deltaLongitude)));
        dist = Math.Acos(dist);
        dist = RadiansToDegrees(dist);
        return (int)(dist * 60 * 1.1515 * 1.609344 * 1000);
    }

    private static double DegreesToRadians(double deg) => deg * Math.PI / 180.0;

    private static double RadiansToDegrees(double rad) => rad / Math.PI * 180.0;
}
