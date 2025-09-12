using FluentAssertions;
using Nhs.Appointments.Api.Models;
using Nhs.Appointments.Api.Validators;
using Nhs.Appointments.Core;

namespace Nhs.Appointments.Api.Tests.Validators;
public class AvailabilityChangeProposalRequestValidatorTests
{
    private readonly AvailabilityChangeProposalRequestValidator _sut = new();

    [Fact]
    public void Validate_ReturnsError_WhenFromIsEmpty()
    {
        var testRequest = new AvailabilityChangeProposalRequest(
            "Site", 
            string.Empty, 
            "2025-10-24", 
            new Session(), 
            new Session()
        );

        var result = _sut.Validate(testRequest);

        result.IsValid.Should().BeFalse();
        result.Errors.Single().PropertyName.Should().Be(nameof(AvailabilityChangeProposalRequest.From));
    }

    [Fact]
    public void Validate_ReturnsError_WhenToIsEmpty()
    {
        var testRequest = new AvailabilityChangeProposalRequest(
            "Site",
            "2025-10-24",
            string.Empty,
            new Session(),
            new Session()
        );

        var result = _sut.Validate(testRequest);

        result.IsValid.Should().BeFalse();
        result.Errors.Single().PropertyName.Should().Be(nameof(AvailabilityChangeProposalRequest.To));
    }

    [Fact]
    public void Validate_ReturnsError_WhenSiteIsEmpty()
    {
        var testRequest = new AvailabilityChangeProposalRequest(
            string.Empty,
            "2025-10-24",
            "2025-10-24",
            new Session(),
            new Session()
        );

        var result = _sut.Validate(testRequest);

        result.IsValid.Should().BeFalse();
        result.Errors.Single().PropertyName.Should().Be(nameof(AvailabilityChangeProposalRequest.Site));
    }

    [Fact]
    public void Validate_ReturnsError_WhenSessionMatcherIsEmpty()
    {
        var testRequest = new AvailabilityChangeProposalRequest(
            "Site",
            "2025-10-24",
            "2025-10-24",
            null,
            new Session()
        );

        var result = _sut.Validate(testRequest);

        result.IsValid.Should().BeFalse();
        result.Errors.Single().PropertyName.Should().Be(nameof(AvailabilityChangeProposalRequest.SessionMatcher));
    }

    [Fact]
    public void Validate_ReturnsError_WhenSessionReplacementIsEmpty()
    {
        var testRequest = new AvailabilityChangeProposalRequest(
            "Site",
            "2025-10-24",
            "2025-10-24",
            new Session(),
            null
        );

        var result = _sut.Validate(testRequest);

        result.IsValid.Should().BeFalse();
        result.Errors.Single().PropertyName.Should().Be(nameof(AvailabilityChangeProposalRequest.SessionReplacement));
    }
}
