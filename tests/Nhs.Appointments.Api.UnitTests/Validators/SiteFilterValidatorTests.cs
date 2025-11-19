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

    [Theory]
    [InlineData(300)]
    [InlineData(300000)]
    public void FailsValidation_WhenSearchRadiusOutOfRange(int radius)
    {
        var filter = new SiteFilter
        {
            Longitude = 5.4,
            Latitude = 50.7,
            SearchRadius = radius
        };

        var result = _sut.Validate(filter);

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void FailsValidation_WhenOnlyOneServiceFilterIsProvided()
    {
        var filter = new SiteFilter
        {
            Longitude = 123.4,
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
    public void FailsValidation_WhenOnlyFromTimeIsProvided()
    {
        var filter = new SiteFilter
        {
            Longitude = 123.4,
            Latitude = 50,
            SearchRadius = 3000,
            Availability = new()
            {
                From = new DateOnly(2024, 9, 1),
            }
        };

        var result = _sut.Validate(filter);

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void FailsValidation_WhenOnlyUntilTimeIsProvided()
    {
        var filter = new SiteFilter
        {
            Longitude = 123.4,
            Latitude = 50,
            SearchRadius = 3000,
            Availability = new()
            {
                Until = new DateOnly(2024, 9, 1),
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
            Longitude = 123.4,
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
            Longitude = 123.4,
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
            Longitude = 123.4,
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
            Longitude = 123.4,
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
    public void FailsValidation_WhenRadiusIsNotProvided()
    {
        var filter = new SiteFilter
        {
            Longitude = 123.4,
            Latitude = 50,
            Availability = new()
            {
                Services = ["RSV:Adult"],
                From = new DateOnly(2025, 9, 2),
                Until = new DateOnly(2025, 9, 15),
            },
            AccessNeeds = ["test_access_need"]
        };

        var result = _sut.Validate(filter);

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void FailsValidation_WhenLongitudeIsNotProvided()
    {
        var filter = new SiteFilter
        {
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
    }

    [Fact]
    public void FailsValdiation_WhenLatitudeIsNotProvided()
    {
        var filter = new SiteFilter
        {
            Longitude = 123.4,
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
