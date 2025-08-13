using FluentAssertions;
using Nhs.Appointments.Api.Models;
using Nhs.Appointments.Api.Validators;

namespace Nhs.Appointments.Api.Tests.Validators;

public class GetSitesByAreaRequestValidatorTests
{
    private readonly GetSitesByAreaRequestValidator _sut = new();

    private const string PartialErrorMessage =
        "All of the 'supports service filters' (services, from, until) must be provided, or none of them.";

    [Theory]
    [InlineData(181)]
    [InlineData(-181)]
    public void Validate_ReturnsError_WhenLongitudeIsInvalid(double longitude)
    {
        var request = new GetSitesByAreaRequest(
            longitude,
            0.123,
            50000,
            50,
            ["access_need_a", "access_need_b"],
            false,
            null,
            null,
            null
        );
        
        var result = _sut.Validate(request);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(1);
        result.Errors.Single().PropertyName.Should().Be(nameof(GetSitesByAreaRequest.Longitude));
    }
    
    [Theory]
    [InlineData(91)]
    [InlineData(-91)]
    public void Validate_ReturnsError_WhenLatitudeIsInvalid(double latitude)
    {
        var request = new GetSitesByAreaRequest(
            0.123,
            latitude,
            50000,
            50,
            ["access_need_a", "access_need_b"],
            false,
            null,
            null,
            null
        );
        
        var result = _sut.Validate(request);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(1);
        result.Errors.Single().PropertyName.Should().Be(nameof(GetSitesByAreaRequest.Latitude));
    }
    
    [Theory]
    [InlineData(999)]
    [InlineData(100001)]
    public void Validate_ReturnsError_WhenSearchRadiusIsInvalid(int searchRadius)
    {
        var request = new GetSitesByAreaRequest(
            0.123,
            0.456,
            searchRadius,
            50,
            ["access_need_a", "access_need_b"],
            false,
            null,
            null,
            null
        );
        
        var result = _sut.Validate(request);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(1);
        result.Errors.Single().PropertyName.Should().Be(nameof(GetSitesByAreaRequest.SearchRadius));
    }
    
    [Theory]
    [InlineData(0)]
    [InlineData(51)]
    public void Validate_ReturnsError_WhenMaxRecordsIsInvalid(int maxRecords)
    {
        var request = new GetSitesByAreaRequest(
            0.123,
            0.456,
            50000,
            maxRecords,
            ["access_need_a", "access_need_b"],
            false,
            null,
            null,
            null
        );
        
        var result = _sut.Validate(request);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(1);
        result.Errors.Single().PropertyName.Should().Be(nameof(GetSitesByAreaRequest.MaximumRecords));
    }

    [Theory]
    [InlineData(180, 90, 100000, 50)]
    [InlineData(-180, -90, 1000, 1)]
    public void Validate_ReturnsSuccess_WhenRequestIsValid(double longitude, double latitude, int searchRadius, int maxRecords)
    {
        var request = new GetSitesByAreaRequest(
            longitude,
            latitude,
            searchRadius,
            maxRecords,
            ["access_need_a", "access_need_b"],
            false,
            null,
            null,
            null
        );
        var result = _sut.Validate(request);
        result.IsValid.Should().BeTrue();
        result.Errors.Should().HaveCount(0);            
    }
    
    [Fact]
    public void Validate_ReturnsSuccess_WhenRequestIsValid_SiteSupportService()
    {
        var request = new GetSitesByAreaRequest(
            180,
            90,
            6000,
            50,
            ["access_need_a", "access_need_b"],
            false,
            ["RSV:Adult"],
            "2025-10-02",
            "2025-10-30"
        );
        var result = _sut.Validate(request);
        result.IsValid.Should().BeTrue();
        result.Errors.Should().HaveCount(0);            
    }
    
    /// <summary>
    /// This might be relaxed later when multiple service querying is supported
    /// </summary>
    [Fact]
    public void Validate_ReturnsError_SiteSupportService_MultipleServicesProvided()
    {
        var request = new GetSitesByAreaRequest(
            180,
            90,
            6000,
            50,
            ["access_need_a", "access_need_b"],
            false,
            ["RSV:Adult", "COVID:19"],
            "2025-10-02",
            "2025-10-30"
        );
        var result = _sut.Validate(request);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(1);
        result.Errors.Single().ErrorMessage.Should().Be("'Services' currently only supports one service: 'RSV:Adult'");     
    }
    
    /// <summary>
    /// APPT-1249 - Restrict to only hard-coded 'RSV:Adult' for now.
    /// </summary>
    [Theory]
    [InlineData("COVID:5_11")]
    [InlineData("COVID:12_17")]
    [InlineData("COVID:18+")]
    [InlineData("FLU:18_64")]
    [InlineData("FLU:65+")]
    [InlineData("COVID_FLU:18_64")]
    [InlineData("COVID_FLU:65+")]
    [InlineData("FLU:2_3")]
    public void Validate_ReturnsError_SiteSupportService_UnsupportedServiceProvided(string service)
    {
        var request = new GetSitesByAreaRequest(
            180,
            90,
            6000,
            50,
            ["access_need_a", "access_need_b"],
            false,
            [service],
            "2025-10-02",
            "2025-10-30"
        );
        var result = _sut.Validate(request);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(1);
        result.Errors.Single().ErrorMessage.Should().Be("'Services' currently only supports one service: 'RSV:Adult'");     
    }
    
    [Fact]
    public void Validate_ReturnsError_SiteSupportService_FromDateFormatting()
    {
        var request = new GetSitesByAreaRequest(
            180,
            90,
            6000,
            50,
            ["access_need_a", "access_need_b"],
            false,
            ["RSV:Adult"],
            "20251002",
            "2025-10-30"
        );
        var result = _sut.Validate(request);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(1);
        result.Errors.Single().ErrorMessage.Should().Be("'From' must be a date in the format 'yyyy-MM-dd'");     
    }
    
    [Fact]
    public void Validate_ReturnsError_SiteSupportService_UntilDateFormatting()
    {
        var request = new GetSitesByAreaRequest(
            180,
            90,
            6000,
            50,
            ["access_need_a", "access_need_b"],
            false,
            ["RSV:Adult"],
            "2025-10-02",
            "20251030"
        );
        var result = _sut.Validate(request);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(1);
        result.Errors.Single().ErrorMessage.Should().Be("'Until' must be a date in the format 'yyyy-MM-dd'");     
    }
    
    [Theory]
    [InlineData(180, 90, 100000, 50)]
    [InlineData(-180, -90, 1000, 1)]
    public void Validate_ReturnsSuccess_WhenRequestIsValid_EmptyServices(double longitude, double latitude, int searchRadius, int maxRecords)
    {
        var request = new GetSitesByAreaRequest(
            longitude,
            latitude,
            searchRadius,
            maxRecords,
            ["access_need_a", "access_need_b"],
            false,
            [],
            null,
            null
        );
        var result = _sut.Validate(request);
        result.IsValid.Should().BeTrue();
        result.Errors.Should().HaveCount(0);            
    }
    
    [Fact]
    public void Validate_ReturnsError_WhenPartialSupportsServiceFiltersProvided_ServicesOnly()
    {
        var request = new GetSitesByAreaRequest(
            0.123,
            0.456,
            50000,
            50,
            ["access_need_a", "access_need_b"],
            false,
            ["RSV:Adult"],
            null,
            null
        );
        
        var result = _sut.Validate(request);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(1);
        result.Errors.Single().ErrorMessage.Should().Be(PartialErrorMessage);
    }
    
    [Fact]
    public void Validate_ReturnsError_WhenPartialSupportsServiceFiltersProvided_FromOnly()
    {
        var request = new GetSitesByAreaRequest(
            0.123,
            0.456,
            50000,
            50,
            ["access_need_a", "access_need_b"],
            false,
            null,
            "2025-10-03",
            null
        );
        
        var result = _sut.Validate(request);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(1);
        result.Errors.Single().ErrorMessage.Should().Be(PartialErrorMessage);
    }
    
    [Fact]
    public void Validate_ReturnsError_WhenPartialSupportsServiceFiltersProvided_UntilOnly()
    {
        var request = new GetSitesByAreaRequest(
            0.123,
            0.456,
            50000,
            50,
            ["access_need_a", "access_need_b"],
            false,
            null,
            null,
            "2025-10-03"
        );
        
        var result = _sut.Validate(request);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(1);
        result.Errors.Single().ErrorMessage.Should().Be(PartialErrorMessage);
    }
    
    [Fact]
    public void Validate_ReturnsError_WhenPartialSupportsServiceFiltersProvided_ServicesAndFromOnly()
    {
        var request = new GetSitesByAreaRequest(
            0.123,
            0.456,
            50000,
            50,
            ["access_need_a", "access_need_b"],
            false,
            ["RSV:Adult"],
            "2025-08-20",
            null
        );
        
        var result = _sut.Validate(request);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(1);
        result.Errors.Single().ErrorMessage.Should().Be(PartialErrorMessage);
    }
    
    [Fact]
    public void Validate_ReturnsError_WhenPartialSupportsServiceFiltersProvided_ServicesAndUntilOnly()
    {
        var request = new GetSitesByAreaRequest(
            0.123,
            0.456,
            50000,
            50,
            ["access_need_a", "access_need_b"],
            false,
            ["RSV:Adult"],
            null,
            "2025-08-20"
        );
        
        var result = _sut.Validate(request);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(1);
        result.Errors.Single().ErrorMessage.Should().Be(PartialErrorMessage);
    }
    
    [Fact]
    public void Validate_ReturnsError_WhenPartialSupportsServiceFiltersProvided_FromAndUntilOnly()
    {
        var request = new GetSitesByAreaRequest(
            0.123,
            0.456,
            50000,
            50,
            ["access_need_a", "access_need_b"],
            false,
            null,
            "2025-08-18",
            "2025-08-20"
        );
        
        var result = _sut.Validate(request);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(1);
        result.Errors.Single().ErrorMessage.Should().Be(PartialErrorMessage);
    }
    
    [Fact]
    public void Validate_ReturnsError_WhenPartialSupportsServiceFiltersProvided_UntilDateLessThanFromDate()
    {
        var request = new GetSitesByAreaRequest(
            0.123,
            0.456,
            50000,
            50,
            ["access_need_a", "access_need_b"],
            false,
            ["RSV:Adult"],
            "2025-08-18",
            "2025-08-17"
        );
        
        var result = _sut.Validate(request);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(1);
        result.Errors.Single().ErrorMessage.Should().Be("'Until' date must be greater than or equal to 'From' date");
    }
    
    [Fact]
    public void Validate_ReturnsSuccess_WhenPartialSupportsServiceFiltersProvided_UntilDateEqualToFromDate()
    {
        var request = new GetSitesByAreaRequest(
            0.123,
            0.456,
            50000,
            50,
            ["access_need_a", "access_need_b"],
            false,
            ["RSV:Adult"],
            "2025-08-18",
            "2025-08-18"
        );
        
        var result = _sut.Validate(request);
        result.IsValid.Should().BeTrue();
    }
}
