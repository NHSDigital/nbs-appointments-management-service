using FluentAssertions;
using Nhs.Appointments.Api.Models;
using Nhs.Appointments.Api.Validators;

namespace Nhs.Appointments.Api.Tests.Validators
{
    public class GetDailyAvailabilityRequestValidatorTests
    {
        private readonly GetDailyAvailabilityRequestValidator _sut = new();

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData(null)]
        public void FailsValidation_WhenSiteIsEmpty(string site)
        {
            var request = new GetDailyAvailabilityRequest(site, "2024-12-01", "2024-12-10");
            var result = _sut.Validate(request);

            result.IsValid.Should().BeFalse();
            result.Errors.Count.Should().Be(1);
            result.Errors.Single().PropertyName.Should().Be(nameof(GetDailyAvailabilityRequest.Site));
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData(null)]
        public void FailsValidation_WhenFromDateNotProvided(string from)
        {
            var request = new GetDailyAvailabilityRequest("TEST01", from, "2024-12-10");
            var result = _sut.Validate(request);

            result.IsValid.Should().BeFalse();
            result.Errors.Count.Should().Be(1);
            result.Errors.Single().PropertyName.Should().Be(nameof(GetDailyAvailabilityRequest.From));
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData(null)]
        public void FailsValidation_WhenToDateNotProvided(string to)
        {
            var request = new GetDailyAvailabilityRequest("TEST01", "2024-12-10", to);
            var result = _sut.Validate(request);

            result.IsValid.Should().BeFalse();
            result.Errors.Count.Should().Be(2);
            result.Errors.First().PropertyName.Should().Be(nameof(GetDailyAvailabilityRequest.From));
            result.Errors.Last().PropertyName.Should().Be(nameof(GetDailyAvailabilityRequest.Until));
        }

        [Fact]
        public void FailsValidation_WhenFromDateIsAfterToDate()
        {
            var request = new GetDailyAvailabilityRequest("TEST01", "2024-12-10", "2024-12-01");
            var result = _sut.Validate(request);

            result.IsValid.Should().BeFalse();
            result.Errors.Count.Should().Be(1);
            result.Errors.Single().PropertyName.Should().Be(nameof(GetDailyAvailabilityRequest.From));
        }

        [Fact]
        public void PassesValidation()
        {
            var request = new GetDailyAvailabilityRequest("TEST01", "2024-12-01", "2024-12-10");
            var result = _sut.Validate(request);

            result.IsValid.Should().BeTrue();
        }
    }
}
