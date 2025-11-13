using FluentAssertions;
using Nhs.Appointments.Api.Models;
using Nhs.Appointments.Api.Validators;
using Nhs.Appointments.Core.Availability;

namespace Nhs.Appointments.Api.Tests.Validators;
public class SessionOrWildcardValidatorTests
{
    private readonly SessionOrWildcardValidator _sut;

    public SessionOrWildcardValidatorTests()
    {
        _sut = new SessionOrWildcardValidator();
    }

    [Fact]
    public void PassesValidation_WhenIsWildcard()
    {
        var request = new SessionOrWildcard
        {
            IsWildcard = true
        };

        var result = _sut.Validate(request);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void FailsValidation_WhenIswildcard()
    {
        var request = new SessionOrWildcard
        {
            IsWildcard = true,
            Session = new Session
            {
                From = new TimeOnly(09, 00),
                Until = new TimeOnly(10, 00),
                SlotLength = 5,
                Capacity = 2,
                Services = ["Service 1"]
            }
        };

        var result = _sut.Validate(request);

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void PassesValidation_WhenIsNotWildcard()
    {
        var request = new SessionOrWildcard
        {
            IsWildcard = false,
            Session = new Session
            {
                From = new TimeOnly(09, 00),
                Until = new TimeOnly(10, 00),
                SlotLength = 5,
                Capacity = 2,
                Services = ["Service 1"]
            }
        };

        var result = _sut.Validate(request);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void FailsValidation_WhenIsWildcard()
    {
        var request = new SessionOrWildcard
        {
            IsWildcard = false
        };

        var result = _sut.Validate(request);

        result.IsValid.Should().BeFalse();
    }
}
