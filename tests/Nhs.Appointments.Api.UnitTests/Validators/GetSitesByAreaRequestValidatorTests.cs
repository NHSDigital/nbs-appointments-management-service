using FluentAssertions;
using Nhs.Appointments.Api.Models;
using Nhs.Appointments.Api.Validators;

namespace Nhs.Appointments.Api.Tests.Validators;

public class GetSitesByAreaRequestValidatorTests
{
    private readonly GetSitesByAreaRequestValidator _sut = new();

    [Theory]
    [InlineData(181)]
    [InlineData(-181)]
    public void Validate_ReturnsError_WhenLongitudeIsInvalid(double longitude)
    {
        var request = new GetSitesRequest(
            longitude,
            0.123,
            50000,
            50
        );
        
        var result = _sut.Validate(request);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(1);
        result.Errors.Single().PropertyName.Should().Be(nameof(GetSitesRequest.longitude));
    }
    
    [Theory]
    [InlineData(91)]
    [InlineData(-91)]
    public void Validate_ReturnsError_WhenLatitudeIsInvalid(double latitude)
    {
        var request = new GetSitesRequest(
            0.123,
            latitude,
            50000,
            50
        );
        
        var result = _sut.Validate(request);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(1);
        result.Errors.Single().PropertyName.Should().Be(nameof(GetSitesRequest.latitude));
    }
    
    [Theory]
    [InlineData(999)]
    [InlineData(100001)]
    public void Validate_ReturnsError_WhenSearchRadiusIsInvalid(int searchRadius)
    {
        var request = new GetSitesRequest(
            0.123,
            0.456,
            searchRadius,
            50
        );
        
        var result = _sut.Validate(request);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(1);
        result.Errors.Single().PropertyName.Should().Be(nameof(GetSitesRequest.searchRadius));
    }
    
    [Theory]
    [InlineData(0)]
    [InlineData(51)]
    public void Validate_ReturnsError_WhenMaxRecordsIsInvalid(int maxRecords)
    {
        var request = new GetSitesRequest(
            0.123,
            0.456,
            50000,
            maxRecords
        );
        
        var result = _sut.Validate(request);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(1);
        result.Errors.Single().PropertyName.Should().Be(nameof(GetSitesRequest.maximumRecords));
    }

    [Theory]
    [InlineData(180, 90, 100000, 50)]
    [InlineData(-180, -90, 1000, 1)]
    public void Validate_ReturnsSuccess_WhenRequestIsValid(double longitude, double latitude, int searchRadius, int maxRecords)
    {
        var request = new GetSitesRequest(
            longitude,
            latitude,
            searchRadius,
            maxRecords
        );
        var result = _sut.Validate(request);
        result.IsValid.Should().BeTrue();
        result.Errors.Should().HaveCount(0);            
    }
}
