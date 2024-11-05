using FluentAssertions;
using Nhs.Appointments.Api.Availability;
using Nhs.Appointments.Api.Validators;
using Nhs.Appointments.Core;
using Moq;

namespace Nhs.Appointments.Api.Tests.Validators
{
    public class SetAvailabilityRequestValidatorTests
    {
        private readonly Mock<TimeProvider> _timeProvider = new();
        private readonly SetAvailabilityRequestValidator _sut;

        public SetAvailabilityRequestValidatorTests()
        {
            _timeProvider
                .Setup(x => x.GetUtcNow())
                .Returns(new DateTimeOffset(DateTime.Parse("2024-10-09T00:00:00Z")));

            _sut = new SetAvailabilityRequestValidator(_timeProvider.Object);
        }

        [Fact]
        public void ReturnsError_WhenDateIsEmpty()
        {
            var request = new SetAvailabilityRequest(
                string.Empty,
                "test-site",
                SetupValidSessions());

            var result = _sut.Validate(request);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().HaveCount(1);
            result.Errors.Single().PropertyName.Should().Be(nameof(SetAvailabilityRequest.Date));
        }

        [Theory]
        [InlineData("10/10/2024")]
        [InlineData("10-10-2024")]
        [InlineData("2024/10/10")]
        public void ReturnsError_WhenDateIsInIncorrectFormat(string date)
        {
            var request = new SetAvailabilityRequest(
                date,
                "test-site",
                SetupValidSessions());

            var result = _sut.Validate(request);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().HaveCount(1);
            result.Errors.Single().PropertyName.Should().Be(nameof(SetAvailabilityRequest.Date));
        }

        [Theory]
        [InlineData(" ")]
        [InlineData("")]
        [InlineData(null)]
        public void ReturnsError_WhenNoSiteSupplied(string site)
        {
            var request = new SetAvailabilityRequest(
                "2024-10-10",
                site,
                SetupValidSessions());

            var result = _sut.Validate(request);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().HaveCount(1);
            result.Errors.Single().PropertyName.Should().Be(nameof(SetAvailabilityRequest.Site));
        }

        [Fact]
        public void ReturnsError_WhenNoSessionsProvided()
        {
            var request = new SetAvailabilityRequest(
                "2024-10-10",
                "test-site",
                Array.Empty<Session>());

            var result = _sut.Validate(request);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().HaveCount(1);
            result.Errors.Single().PropertyName.Should().Be(nameof(SetAvailabilityRequest.Sessions));
        }

        [Fact]
        public void ReturnsValid_WhenRequestIsValid()
        {
            var request = new SetAvailabilityRequest(
                "2024-10-10",
                "test-site",
                SetupValidSessions());

            var result = _sut.Validate(request);

            result.IsValid.Should().BeTrue();
        }

        [Fact]
        public void ReturnsError_WhenDateIsMoreThanOneYearInTheFuture()
        {
            var request = new SetAvailabilityRequest(
                "2026-01-01",
                "test-site",
                SetupValidSessions());

            var result = _sut.Validate(request);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().HaveCount(1);
            result.Errors.Single().PropertyName.Should().Be(nameof(SetAvailabilityRequest.Date));
            result.Errors.Single().ErrorMessage.Should().Be("Date cannot be later than 1 year from now");
        }

        [Fact]
        public void ReturnsError_WhenDateIsTodayOrEarlier()
        {
            var request = new SetAvailabilityRequest(
                "2024-10-09",
                "test-site",
                SetupValidSessions());

            var result = _sut.Validate(request);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().HaveCount(1);
            result.Errors.Single().PropertyName.Should().Be(nameof(SetAvailabilityRequest.Date));
            result.Errors.Single().ErrorMessage.Should().Be("Date must be at least 1 day in the future");
        }

        private static Session[] SetupValidSessions()
        {
            return
            [
                new()
                {
                    Capacity = 1,
                    From = TimeOnly.FromDateTime(new DateTime(2024, 10, 10, 9, 0, 0)),
                    Until = TimeOnly.FromDateTime(new DateTime(2024, 10, 10, 16, 0, 0)),
                    Services = ["RSV", "COVID"],
                    SlotLength = 5
                }
            ];
        }
    }
}
