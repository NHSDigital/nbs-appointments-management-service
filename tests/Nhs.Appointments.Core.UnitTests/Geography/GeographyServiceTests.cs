using Nhs.Appointments.Core.Geography;

namespace Nhs.Appointments.Core.UnitTests.Geography;

public class GeographyServiceTests
{
    private readonly GeographyService _sut = new();

    [Theory]
    // Same coords, no distance
    [InlineData(0.0, 0.0, 0.0, 0.0, 0)]
    [InlineData(51.5074, -0.1278, 51.5074, -0.1278, 0)]

    // Between two points on Aire St, in both directions
    [InlineData(53.79575967529175, -1.550056172409011, 53.795892571140456, -1.5513165165237766, 84)]
    [InlineData(53.795892571140456, -1.5513165165237766, 53.79575967529175, -1.550056172409011, 84)]

    // Manchester Cathedral to Piccadilly Gardens, in both directions
    [InlineData(53.48519226511998, -2.244328738332362, 53.480833024954016, -2.2371119243182, 680)]
    [InlineData(53.480833024954016, -2.2371119243182, 53.48519226511998, -2.244328738332362, 680)]

    // Belfast Cathedral to Bridge of Sighs, in both directions
    [InlineData(54.60289987213229, -5.928455876064277, 52.20850298428771, 0.11577497467868834, 480777)]
    [InlineData(52.20850298428771, 0.11577497467868834, 54.60289987213229, -5.928455876064277, 480777)]
    public void CalculatesDistance(double originLat, double originLong, double destinationLat, double destinationLong,
        int expectedDistance)
    {
        var originCoordinates = new Coordinates { Latitude = originLat, Longitude = originLong };
        var destinationCoordinates = new Coordinates { Latitude = destinationLat, Longitude = destinationLong };

        var result = _sut.CalculateDistanceInMetres(originCoordinates, destinationCoordinates);

        result.Should().Be(expectedDistance);
    }
}
