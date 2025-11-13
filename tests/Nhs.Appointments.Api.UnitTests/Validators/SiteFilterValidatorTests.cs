using FluentAssertions;
using Moq;
using Nhs.Appointments.Api.Validators;
using Nhs.Appointments.Core.Sites;

namespace Nhs.Appointments.Api.Tests.Validators;
public class SiteFilterValidatorTests
{
    private readonly Mock<TimeProvider> _timeProvider = new();
    private readonly SiteFilterValidator _sut;

    public SiteFilterValidatorTests()
    {
        _timeProvider.Setup(x => x.GetUtcNow())
            .Returns(new DateTimeOffset(DateTime.Parse("2025-09-01T00:00:00Z")));

        _sut = new SiteFilterValidator(_timeProvider.Object);
    }

    [Fact]
    public void FailsValidation_WhenLongitudeOutOfRange()
    {
        var filter = new SiteFilter
        {
            Longitude = 223.4,
            Latitude = 50.7,
            SearchRadius = 3000
        };

        var result = _sut.Validate(filter);

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void FailsValidation_WhenLatitudeOutOfRange()
    {
        var filter = new SiteFilter
        {
            Longitude = 50.7,
            Latitude = -100.7,
            SearchRadius = 3000
        };

        var result = _sut.Validate(filter);

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void FailsValidation_WhenSearchRadiusOutOfRange()
    {
        var filter = new SiteFilter
        {
            Longitude = 5.4,
            Latitude = 50.7,
            SearchRadius = 300
        };

        var result = _sut.Validate(filter);

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void FailsValidation_WhenOnlyOneServiceFilterIsProvided()
    {
        var filter = new SiteFilter
        {
            Longitude = 1234,
            Latitude = 50,
            SearchRadius = 3000,
            Availability = new()
            {
                Services = ["test_service"]
            }
        };

        var result = _sut.Validate(filter);

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void FailsValidation_WhenFromServiceFilterIsInThePast()
    {
        var filter = new SiteFilter
        {
            Longitude = 1234,
            Latitude = 50,
            SearchRadius = 3000,
            Availability = new()
            {
                Services = ["test_service"],
                From = new DateOnly(2024, 9, 1),
                Until = new DateOnly(2025, 10, 1)
            }
        };

        var result = _sut.Validate(filter);

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void FailsValidation_WhenUntilServiceFilterIsInThePast()
    {
        var filter = new SiteFilter
        {
            Longitude = 1234,
            Latitude = 50,
            SearchRadius = 3000,
            Availability = new()
            {
                Services = ["test_service"],
                From = new DateOnly(2025, 9, 10),
                Until = new DateOnly(2024, 10, 1)
            }
        };

        var result = _sut.Validate(filter);

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void FailsValidation_WhenFromServiceFilterIsAfterUntilFilter()
    {
        var filter = new SiteFilter
        {
            Longitude = 1234,
            Latitude = 50,
            SearchRadius = 3000,
            Availability = new()
            {
                Services = ["test_service"],
                From = new DateOnly(2025, 10, 10),
                Until = new DateOnly(2025, 10, 1)
            }
        };

        var result = _sut.Validate(filter);

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void FailsValidation_WhenUntilFilterIsBeforeFromFilter()
    {
        var filter = new SiteFilter
        {
            Longitude = 1234,
            Latitude = 50,
            SearchRadius = 3000,
            Availability = new()
            {
                Services = ["test_service"],
                From = new DateOnly(2025, 9, 2),
                Until = new DateOnly(2025, 8, 1)
            }
        };

        var result = _sut.Validate(filter);

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void PassessValidation()
    {
        var filter = new SiteFilter
        {
            Longitude = 123.4,
            Latitude = 50,
            SearchRadius = 3000,
            Availability = new()
            {
                Services = ["RSV:Adult"],
                From = new DateOnly(2025, 9, 2),
                Until = new DateOnly(2025, 9, 15),
            },
            AccessNeeds = ["test_access_need"]
        };

        var result = _sut.Validate(filter);

        result.IsValid.Should().BeTrue();
    }
}
