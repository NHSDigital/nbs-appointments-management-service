using FluentAssertions;
using Nhs.Appointments.Api.Models;
using Nhs.Appointments.Api.Validators;

namespace Nhs.Appointments.Api.Tests.Validators;

public class QueryBookingsRequestValidatorTests
{
    private readonly QueryBookingsRequestValidator _sut = new();

    [Fact]
    public void Validate_ReturnError_WhenSiteIsBlank()
    {
        var testRequest = new QueryBookingsRequest(DateTime.Today, DateTime.Today.AddDays(1), string.Empty);
        var result = _sut.Validate(testRequest);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(1);
        result.Errors.Single().PropertyName.Should().Be(nameof(QueryBookingsRequest.site));
    }

    [Fact]
    public void Validate_ReturnError_WhenDateRangeIsInvalid()
    {
        var testRequest = new QueryBookingsRequest(DateTime.Today.AddDays(1), DateTime.Today, "site");
        var result = _sut.Validate(testRequest);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(1);
        result.Errors.Single().PropertyName.Should().Be(nameof(QueryBookingsRequest.from));
    }

    [Fact]
    public void Validate_ReturnsSuccess_WhenRequestWithStatusesIsValid()
    {
        var testRequest = new QueryBookingsRequest(
            DateTime.Today,
            DateTime.Today.AddDays(1),
            "site", ["Cancelled", "Booked"]);

        var result = _sut.Validate(testRequest);

        result.IsValid.Should().BeTrue();
        result.Errors.Should().HaveCount(0);
    }

    [Fact]
    public void Validate_ReturnsError_WhenRequestWithStatusesIsInvalid()
    {
        var testRequest = new QueryBookingsRequest(
            DateTime.Today,
            DateTime.Today.AddDays(1),
            "site", ["Cancelled", "Booked", "NOT A STATUS"]);

        var result = _sut.Validate(testRequest);

        result.IsValid.Should().BeFalse();
        result.Errors.Single().ErrorMessage.Should().Be("Provide valid appointment statuses");
    }

    [Fact]
    public void Validate_ReturnsSuccess_WhenRequestWithCancellationReasonIsValid()
    {
        var testRequest = new QueryBookingsRequest(
            DateTime.Today,
            DateTime.Today.AddDays(1),
            "site", null, "CancelledByCitizen");

        var result = _sut.Validate(testRequest);

        result.IsValid.Should().BeTrue();
        result.Errors.Should().HaveCount(0);
    }

    [Fact]
    public void Validate_ReturnsError_WhenRequestWithCancellationReasonIsInvalid()
    {
        var testRequest = new QueryBookingsRequest(
            DateTime.Today,
            DateTime.Today.AddDays(1),
            "site", null, "NOT A CANCELLATION REASON");

        var result = _sut.Validate(testRequest);

        result.IsValid.Should().BeFalse();
        result.Errors.Single().ErrorMessage.Should().Be("Provide a valid cancellation reason");
    }

    [Fact]
    public void Validate_ReturnsSuccess_WhenRequestWithCancellationNotificationStatusesIsValid()
    {
        var testRequest = new QueryBookingsRequest(
            DateTime.Today,
            DateTime.Today.AddDays(1),
            "site", null, null, ["Unnotified", "Notified"]);

        var result = _sut.Validate(testRequest);

        result.IsValid.Should().BeTrue();
        result.Errors.Should().HaveCount(0);
    }

    [Fact]
    public void Validate_ReturnsError_WhenRequestWithCancellationNotificationStatusesIsInvalid()
    {
        var testRequest = new QueryBookingsRequest(
            DateTime.Today,
            DateTime.Today.AddDays(1),
            "site", null, null, ["Unnotified", "Notified", "NOT A STATUS"]);

        var result = _sut.Validate(testRequest);

        result.IsValid.Should().BeFalse();
        result.Errors.Single().ErrorMessage.Should().Be("Provide valid cancellation notification statuses");
    }
}
