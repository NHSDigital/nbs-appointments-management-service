using FluentAssertions;
using Moq;
using Nhs.Appointments.Api.Models;
using Nhs.Appointments.Api.Validators;

namespace Nhs.Appointments.Api.Tests.Validators;
public class CancelDayRequestValidatorTests
{
    private readonly Mock<TimeProvider> _timeProvider = new();
    private readonly CancelDayRequestValidator _sut;

    public CancelDayRequestValidatorTests()
    {
        _timeProvider
            .Setup(x => x.GetUtcNow())
            .Returns(new DateTimeOffset(DateTime.Parse("2076-12-31T00:00:00Z")));

        _sut = new CancelDayRequestValidator(_timeProvider.Object);
    }

    [Fact]
    public void ReturnsValidResult_WhenAllFieldsAreValid()
    {
        var request = new CancelDayRequest(Guid.NewGuid().ToString(), new DateOnly(2077, 01, 01));

        var result = _sut.Validate(request);

        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void ReturnsInvalid_WhenSiteIsInvalid(string siteId)
    {
        var request = new CancelDayRequest(siteId, new DateOnly(2077, 01, 01));

        var result = _sut.Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Count.Should().Be(1);
        result.Errors.First().ErrorMessage.Should().Be("Provide a valid site.");
    }

    [Fact]
    public void Validate_ReturnsError_WhenFromDateIsTodayOrEarlier()
    {
        var request = new CancelDayRequest(Guid.NewGuid().ToString(), new DateOnly(2076, 12, 31));

        var result = _sut.Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Count.Should().Be(1);
        result.Errors.First().ErrorMessage.Should().Be("Date must be in the future.");
    }
}
