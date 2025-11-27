using FluentAssertions;
using Moq;
using Nhs.Appointments.Api.Models;
using Nhs.Appointments.Api.Validators;
using Nhs.Appointments.Core.Availability;

namespace Nhs.Appointments.Api.Tests.Validators;
public class AvailabilityQueryByHoursRequestValidatorTests
{
    private readonly Mock<TimeProvider> _timeProvider = new();
    private readonly AvailabilityQueryByHoursRequestValidator _sut;

    public AvailabilityQueryByHoursRequestValidatorTests()
    {
        _timeProvider.Setup(x => x.GetUtcNow())
            .Returns(new DateTimeOffset(DateTime.Parse("2025-09-01T00:00:00Z")));

        _sut = new AvailabilityQueryByHoursRequestValidator(_timeProvider.Object);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void FailsValidation_WhenSiteIsEmpty(string site)
    {
        var request = new AvailabilityQueryByHoursRequest(
            site,
            [
                new() { Services = ["RSV:Adult"] }
            ],
            new DateOnly(2025, 9, 1));

        var result = _sut.Validate(request);

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void FailsValidation_WhenAttndeeListIsEmpty()
    {
        var request = new AvailabilityQueryByHoursRequest(
            "test-site-123",
            [],
            new DateOnly(2025, 9, 1));

        var result = _sut.Validate(request);

        result.IsValid.Should().BeFalse();
    }

    [Theory]
    [InlineData(6)]
    [InlineData(10)]
    [InlineData(20)]
    public void FailsValidation_WhenThereAreTooManyAttendees(int attendeeCount)
    {
        var attendees = new List<Attendee>();

        for (var i = 0; i < attendeeCount; i++)
        {
            attendees.Add(new Attendee
            {
                Services = ["RSV:Adult"]
            });
        }

        var request = new AvailabilityQueryByHoursRequest(
            "test-site-123",
            attendees,
            new DateOnly(2025, 9, 1));

        var result = _sut.Validate(request);

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void FailsValidation_WhenDateIsInPast()
    {
        var request = new AvailabilityQueryByHoursRequest(
            "test-site-123",
            [
                new() { Services = ["RSV:Adult"] }
            ],
            new DateOnly(2024, 9, 1));

        var result = _sut.Validate(request);

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void FailsValidation_WhenDateNotProvided()
    {
        var request = new AvailabilityQueryByHoursRequest(
            "test-site-123",
            [
                new() { Services = ["RSV:Adult"] }
            ],
            default);

        var result = _sut.Validate(request);

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void PassesValidation()
    {
        var request = new AvailabilityQueryByHoursRequest(
            "test-site-123",
            [
                new() { Services = ["RSV:Adult"] }
            ],
            new DateOnly(2025, 9, 2));

        var result = _sut.Validate(request);

        result.IsValid.Should().BeTrue();
    }
}
