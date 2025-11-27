using FluentAssertions;
using Nhs.Appointments.Api.Validators;
using Nhs.Appointments.Core.Availability;

namespace Nhs.Appointments.Api.Tests.Validators;
public class AttendeeValidatorTests
{
    private readonly AttendeeValidator _sut = new();

    [Fact]
    public void FailsValidation_WhenServicesListIsEmpty()
    {
        var attendee = new Attendee { Services = [] };

        var result = _sut.Validate(attendee);

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void PassesValidation()
    {
        var attendee = new Attendee { Services = ["RSV:Adult"] };

        var result = _sut.Validate(attendee);

        result.IsValid.Should().BeTrue();
    }
}
