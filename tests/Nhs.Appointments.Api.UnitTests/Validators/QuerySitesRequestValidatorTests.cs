using FluentAssertions;
using Moq;
using Nhs.Appointments.Api.Models;
using Nhs.Appointments.Api.Validators;
using Nhs.Appointments.Core;

namespace Nhs.Appointments.Api.Tests.Validators;
public class QuerySitesRequestValidatorTests
{
    private readonly Mock<TimeProvider> _timeProvider = new();

    private readonly QuerySitesRequestValidator _sut;

    public QuerySitesRequestValidatorTests()
    {
        _timeProvider.Setup(x => x.GetUtcNow())
            .Returns(new DateTimeOffset(DateTime.Parse("2025-09-01T00:00:00Z")));

        _sut = new QuerySitesRequestValidator(_timeProvider.Object);
    }

    [Fact]
    public void FailsValidation_WhenNoFiltersProvided()
    {
        var request = new QuerySitesRequest([]);

        var result = _sut.Validate(request);

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void FailsValidation_WhenSiteFilterIsInvalid()
    {
        var filters = new SiteFilter[]
        {
            new() 
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
            }
        };

        var request = new QuerySitesRequest(filters);

        var result = _sut.Validate(request);

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void PassesValidation()
    {

        var filters = new SiteFilter[]
        {
            new()
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
            }
        };

        var request = new QuerySitesRequest(filters);

        var result = _sut.Validate(request);

        result.IsValid.Should().BeTrue();
    }
}
