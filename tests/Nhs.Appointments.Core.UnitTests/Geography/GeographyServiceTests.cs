using Nhs.Appointments.Core.Geography;

namespace Nhs.Appointments.Core.UnitTests.Geography;

public class GeographyServiceTests
{
    private readonly GeographyService _sut;

    public GeographyServiceTests() => _sut = new GeographyService();

    [Theory]
    [InlineData(0.0, 0.0, 0.0, 0.0, 0)]
    [InlineData(51.5074, -0.1278, 51.5074, -0.1278, 0)]
    public void CalculatesDistance(double originLat, double originLong, double destinationLat, double destinationLong,
        int expectedDistance)
    {
        var originCoordinates = new Coordinates { Latitude = originLat, Longitude = originLong };
        var destinationCoordinates = new Coordinates { Latitude = destinationLat, Longitude = destinationLong };

        var result = _sut.CalculateDistanceInMetres(originCoordinates, destinationCoordinates);

        result.Should().Be(expectedDistance);
    }
}
