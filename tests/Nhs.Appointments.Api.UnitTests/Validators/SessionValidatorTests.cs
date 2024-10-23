using FluentAssertions;
using FluentValidation.TestHelper;
using Nhs.Appointments.Api.Validators;
using Nhs.Appointments.Core;

namespace Nhs.Appointments.Api.Tests.Validators;

public class SessionValidatorTests
{
    private readonly SessionValidator _sut = new();
    
    [Fact]
    public void Validate_ReturnsValidResult_WhenAllFieldsAreValid()
    {
        var session = new Session()
        {
            Capacity = 1,
            From = new TimeOnly(09, 00),
            Until = new TimeOnly(10, 00),
            SlotLength = 5,
            Services = ["Service 1"]
        };
        var result = _sut.TestValidate(session);
        result.IsValid.Should().BeTrue();
    }
    
    [Theory]
    [InlineData(null)]
    [InlineData(0)]
    public void Validate_ReturnsError_WhenCapacityIsInvalid(int capacity)
    {
        var session = new Session()
        {
            Capacity = capacity,
            From = new TimeOnly(09, 00),
            Until = new TimeOnly(10, 00),
            SlotLength = 5,
            Services = ["Service 1"]
        };
        var result = _sut.TestValidate(session);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(1);
        result.Errors.Single().PropertyName.Should().Contain(nameof(Session.Capacity));
    }
    
    [Theory]
    [InlineData(null)]
    [InlineData(0)]
    public void Validate_ReturnsError_WhenSlotLengthIsInvalid(int slotLength)
    {
        var session = new Session()
        {
            Capacity = 1,
            From = new TimeOnly(09, 00),
            Until = new TimeOnly(10, 00),
            SlotLength = slotLength,
            Services = ["Service 1"]
        };
        var result = _sut.TestValidate(session);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(1);
        result.Errors.Single().PropertyName.Should().Contain(nameof(Session.SlotLength));
    }

    [Fact]
    public void Validate_ReturnsError_WhenServicesIsNull()
    {
        var session = new Session()
        {
            Capacity = 1,
            From = new TimeOnly(09, 00),
            Until = new TimeOnly(10, 00),
            SlotLength = 5,
            Services = null
        };
        var result = _sut.TestValidate(session);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(1);
        result.Errors.Single().PropertyName.Should().Contain(nameof(Session.Services));
    }
    
    [Fact]
    public void Validate_ReturnsError_WhenServicesIsEmpty()
    {
        var session = new Session()
        {
            Capacity = 1,
            From = new TimeOnly(09, 00),
            Until = new TimeOnly(10, 00),
            SlotLength = 5,
            Services = []
        };
        var result = _sut.TestValidate(session);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(1);
        result.Errors.Single().PropertyName.Should().Contain(nameof(Session.Services));
    }
    
    [Theory]
    [InlineData("", "Service-1")]
    [InlineData("")]
    public void Validate_ReturnsError_WhenServicesAreInvalid(params string[] services)
    {
        var session = new Session()
        {
            Capacity = 1,
            From = new TimeOnly(09, 00),
            Until = new TimeOnly(10, 00),
            SlotLength = 5,
            Services = services
        };
        var result = _sut.TestValidate(session);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(1);
        result.Errors.Single().PropertyName.Should().Contain(nameof(Session.Services));
    }
    
    [Fact]
    public void Validate_ReturnsError_WhenFromIsAfterUntil()
    {
        var session = new Session()
        {
            Capacity = 1,
            From = new TimeOnly(10, 01),
            Until = new TimeOnly(10, 00),
            SlotLength = 5,
            Services = ["Service-1"]
        };
        var result = _sut.TestValidate(session);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(1);
        result.Errors.Single().PropertyName.Should().Contain(nameof(Session.From));
    }
    
    [Fact]
    public void Validate_ReturnsError_WhenSessionTimeIsLessThanSlotLength()
    {
        var session = new Session()
        {
            Capacity = 1,
            From = new TimeOnly(10, 00),
            Until = new TimeOnly(10, 04),
            SlotLength = 5,
            Services = ["Service-1"]
        };
        var result = _sut.TestValidate(session);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(1);
        result.Errors.Single().PropertyName.Should().Contain(nameof(Session.Until));
    }
}
