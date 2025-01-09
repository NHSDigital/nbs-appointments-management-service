using FluentAssertions;
using Moq;
using Nhs.Appointments.Api.Availability;
using Nhs.Appointments.Api.Validators;
using Nhs.Appointments.Core;

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

        [Theory]
        [InlineData(" ")]
        [InlineData("")]
        [InlineData(null)]
        public void ReturnsError_WhenNoSiteSupplied(string site)
        {
            var request = new SetAvailabilityRequest(
                new DateOnly(2024, 10, 10),
                site,
                SetupValidSessions(),
                ApplyAvailabilityMode.Overwrite);

            var result = _sut.Validate(request);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().HaveCount(1);
            result.Errors.Single().PropertyName.Should().Be(nameof(SetAvailabilityRequest.Site));
        }

        [Fact]
        public void ReturnsError_WhenNoSessionsProvided()
        {
            var request = new SetAvailabilityRequest(
                new DateOnly(2024, 10, 10),
                "test-site",
                Array.Empty<Session>(),
                ApplyAvailabilityMode.Overwrite);

            var result = _sut.Validate(request);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().HaveCount(1);
            result.Errors.Single().PropertyName.Should().Be(nameof(SetAvailabilityRequest.Sessions));
        }

        [Fact]
        public void ReturnsValid_WhenRequestIsValid()
        {
            var request = new SetAvailabilityRequest(
                new DateOnly(2024, 10, 10),
                "test-site",
                SetupValidSessions(),
                ApplyAvailabilityMode.Overwrite);

            var result = _sut.Validate(request);

            result.IsValid.Should().BeTrue();
        }

        [Fact]
        public void ReturnsError_WhenDateIsMoreThanOneYearInTheFuture()
        {
            var request = new SetAvailabilityRequest(
                new DateOnly(2026, 01, 01),
                "test-site",
                SetupValidSessions(),
                ApplyAvailabilityMode.Overwrite);

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
                new DateOnly(2024, 10, 09),
                "test-site",
                SetupValidSessions(),
                ApplyAvailabilityMode.Overwrite);

            var result = _sut.Validate(request);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().HaveCount(1);
            result.Errors.Single().PropertyName.Should().Be(nameof(SetAvailabilityRequest.Date));
            result.Errors.Single().ErrorMessage.Should().Be("Date must be at least 1 day in the future");
        }

        [Fact]
        public void ReturnsValid_WhenRequestInEditModeIsValid()
        {
            var request = new SetAvailabilityRequest(
                new DateOnly(2024, 10, 10),
                "test-site",
                [
                    new Session
                    {
                        Capacity = 5,
                        From = TimeOnly.FromDateTime(new DateTime(2024, 10, 10, 9, 0, 0)),
                        Until = TimeOnly.FromDateTime(new DateTime(2024, 10, 10, 12, 0, 0)),
                        Services = ["RSV", "COVID"],
                        SlotLength = 5
                    }
                ],
                ApplyAvailabilityMode.Edit,
                SetupValidSessions().First());

            var result = _sut.Validate(request);

            result.IsValid.Should().BeTrue();
        }

        [Fact]
        public void ReturnsError_IfModeIsEditButNoSessionToEditIsProvided()
        {
            var request = new SetAvailabilityRequest(
                new DateOnly(2024, 10, 10),
                "test-site",
                [SetupValidSessions().First()],
                ApplyAvailabilityMode.Edit);

            var result = _sut.Validate(request);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().HaveCount(1);
            result.Errors.Single().PropertyName.Should().Be(nameof(SetAvailabilityRequest.SessionToEdit));
            result.Errors.Single().ErrorMessage.Should()
                .Be("A session to edit must be provided when mode is set to Edit");
        }

        [Fact]
        public void ReturnsError_IfModeIsEditAndMultipleSessionsAreProvided()
        {
            var request = new SetAvailabilityRequest(
                new DateOnly(2024, 10, 10),
                "test-site",
                SetupValidSessions(),
                ApplyAvailabilityMode.Edit,
                SetupValidSessions().First());

            var result = _sut.Validate(request);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().HaveCount(1);
            result.Errors.Single().PropertyName.Should().Be(nameof(SetAvailabilityRequest.Sessions));
            result.Errors.Single().ErrorMessage.Should()
                .Be("Only one edited session can be provided when mode is set to Edit");
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
                },
                new Session
                {
                    Capacity = 2,
                    From = TimeOnly.FromDateTime(new DateTime(2024, 10, 10, 13, 0, 0)),
                    Until = TimeOnly.FromDateTime(new DateTime(2024, 10, 10, 17, 0, 0)),
                    Services = ["RSV"],
                    SlotLength = 5
                }
            ];
        }
    }
}
