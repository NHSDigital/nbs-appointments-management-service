using FluentAssertions;
using Nhs.Appointments.Api.Models;
using Nhs.Appointments.Api.Validators;

namespace Nhs.Appointments.Api.Tests.Validators;

public class GetAvailabilityCreatedEventsRequestValidatorTests
{
    private readonly GetAvailabilityCreatedEventsRequestValidator _sut = new();

    [Fact]
    public void Validate_ReturnsError_WhenSiteIsBlank()
    {
        var testRequest = new GetAvailabilityCreatedEventsRequest(string.Empty, "2020-01-01");
        var result = _sut.Validate(testRequest);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(1);
        result.Errors.Single().PropertyName.Should().Be(nameof(GetAvailabilityCreatedEventsRequest.Site));
    }

    [Fact]
    public void Validate_ReturnsError_WhenSiteIsNull()
    {
        var testRequest = new GetAvailabilityCreatedEventsRequest(null, "2020-01-01");
        var result = _sut.Validate(testRequest);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(1);
        result.Errors.Single().PropertyName.Should().Be(nameof(GetAvailabilityCreatedEventsRequest.Site));
    }

    [Fact]
    public void Validate_ReturnsError_WhenFromIsBlank()
    {
        var testRequest = new GetAvailabilityCreatedEventsRequest("cb5596bc-ef41-42c9-a8f1-57d713d6cc91", "");
        var result = _sut.Validate(testRequest);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(1);
        result.Errors.Single().PropertyName.Should().Be(nameof(GetAvailabilityCreatedEventsRequest.From));
    }

    [Fact]
    public void Validate_ReturnsError_WhenFromIsNull()
    {
        var testRequest = new GetAvailabilityCreatedEventsRequest("cb5596bc-ef41-42c9-a8f1-57d713d6cc91", null);
        var result = _sut.Validate(testRequest);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(1);
        result.Errors.Single().PropertyName.Should().Be(nameof(GetAvailabilityCreatedEventsRequest.From));
    }

    [Fact]
    public void Validate_ReturnsError_WhenFromIsInWrongFormat()
    {
        var testRequest = new GetAvailabilityCreatedEventsRequest("cb5596bc-ef41-42c9-a8f1-57d713d6cc91", "20:01:2031");
        var result = _sut.Validate(testRequest);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(1);
        result.Errors.Single().PropertyName.Should().Be(nameof(GetAvailabilityCreatedEventsRequest.From));
    }

    [Fact]
    public void Validate_ReturnsSuccess_WhenRequestIsValid()
    {
        var testRequest = new GetAvailabilityCreatedEventsRequest("cb5596bc-ef41-42c9-a8f1-57d713d6cc91", "2020-01-01");
        var result = _sut.Validate(testRequest);
        result.IsValid.Should().BeTrue();
        result.Errors.Should().HaveCount(0);
    }
}

