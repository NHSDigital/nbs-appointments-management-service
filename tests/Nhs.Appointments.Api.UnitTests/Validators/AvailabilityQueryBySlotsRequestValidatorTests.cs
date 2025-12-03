using FluentAssertions;
using Moq;
using Nhs.Appointments.Api.Models;
using Nhs.Appointments.Api.Validators;
using Nhs.Appointments.Core.Availability;

namespace Nhs.Appointments.Api.Tests.Validators;
public class AvailabilityQueryBySlotsRequestValidatorTests
{
    private readonly Mock<TimeProvider> _timeProvider = new();
    private readonly AvailabilityQueryBySlotsRequestValidator _sut;

    public AvailabilityQueryBySlotsRequestValidatorTests()
    {
        _timeProvider.Setup(x => x.GetUtcNow())
            .Returns(new DateTimeOffset(DateTime.Parse("2025-09-01T00:00:00Z")));

        _sut = new AvailabilityQueryBySlotsRequestValidator(_timeProvider.Object);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void FailsValidation_WhenSiteIsEmpty(string site)
    {
        var request = new AvailabilityQueryBySlotsRequest(
            site,
            [
                new() { Services = ["RSV:Adult"] }
            ],
            DateTime.Parse("2025-10-01T09:00:00.000Z"),
            DateTime.Parse("2025-10-01T17:00:00.000Z"));

        var result = _sut.Validate(request);

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void FailsValidation_WhenAttendeesArrayIsEmpty()
    {
        var request = new AvailabilityQueryBySlotsRequest(
            "test-site-123",
            [],
            DateTime.Parse("2025-10-01T09:00:00.000Z"),
            DateTime.Parse("2025-10-01T17:00:00.000Z"));

        var result = _sut.Validate(request);

        result.IsValid.Should().BeFalse();
    }

    [Theory]
    [InlineData(6)]
    [InlineData(10)]
    public void FailsValidation_WhenTooManyAttendeesInArray(int attendeeCount)
    {
        var attendeeCollection = new List<Attendee>();

        for (var i = 0; i < attendeeCount; i++)
        {
            attendeeCollection.Add(new() { Services = ["RSV:Adult"] });
        }

        var request = new AvailabilityQueryBySlotsRequest(
            "test-site-123",
            attendeeCollection,
            DateTime.Parse("2025-10-01T09:00:00.000Z"),
            DateTime.Parse("2025-10-01T17:00:00.000Z"));

        var result = _sut.Validate(request);

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void FailsValidation_WhenFromDateNotProvided()
    {
        var request = new AvailabilityQueryBySlotsRequest(
            "test-site-123",
            [
                new() { Services = ["RSV:Adult"] }
            ],
            default,
            DateTime.Parse("2025-10-01T17:00:00.000Z"));

        var result = _sut.Validate(request);

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void FailsValidation_WhenUntilDateNotProvided()
    {
        var request = new AvailabilityQueryBySlotsRequest(
            "test-site-123",
            [
                new() { Services = ["RSV:Adult"] }
            ],
            DateTime.Parse("2025-10-01T09:00:00.000Z"),
            default);

        var result = _sut.Validate(request);

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void FailsValidation_WhenFromDateInThePast()
    {
        var request = new AvailabilityQueryBySlotsRequest(
            "test-site-123",
            [
                new() { Services = ["RSV:Adult"] }
            ],
            DateTime.Parse("2025-08-01T09:00:00.000Z"),
            DateTime.Parse("2025-10-01T16:00:00.000Z"));

        var result = _sut.Validate(request);

        result.IsValid.Should().BeFalse();

    }

    [Fact]
    public void FailsValidation_WhenUntilDateInThePast()
    {
        var request = new AvailabilityQueryBySlotsRequest(
            "test-site-123",
            [
                new() { Services = ["RSV:Adult"] }
            ],
            DateTime.Parse("2025-10-01T09:00:00.000Z"),
            DateTime.Parse("2025-08-01T16:00:00.000Z"));

        var result = _sut.Validate(request);

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void FailsValidation_WhenFromDateAfterUntilDate()
    {
        var request = new AvailabilityQueryBySlotsRequest(
            "test-site-123",
            [
                new() { Services = ["RSV:Adult"] }
            ],
            DateTime.Parse("2025-10-01T09:00:00.000Z"),
            DateTime.Parse("2025-08-01T16:00:00.000Z"));

        var result = _sut.Validate(request);

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void FailsValidation_WhenFromDate_AndUntilDate_NotSameDay()
    {
        var request = new AvailabilityQueryBySlotsRequest(
            "test-site-123",
            [
                new() { Services = ["RSV:Adult"] }
            ],
            DateTime.Parse("2025-10-01T09:00:00.000Z"),
            DateTime.Parse("2025-10-02T16:00:00.000Z"));

        var result = _sut.Validate(request);

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void PassesValidation()
    {
        var request = new AvailabilityQueryBySlotsRequest(
            "test-site-123",
            [
                new() { Services = ["RSV:Adult"] }
            ],
            DateTime.Parse("2025-10-01T09:00:00.000Z"),
            DateTime.Parse("2025-10-01T16:00:00.000Z"));

        var result = _sut.Validate(request);

        result.IsValid.Should().BeTrue();
    }
}
