using FluentAssertions;
using Nhs.Appointments.Api.Models;
using Nhs.Appointments.Api.Validators;

namespace Nhs.Appointments.Api.Tests.Validators;

public class MakeBookingRequestValidatorTests
{
    private readonly MakeBookingRequestValidator _sut = new();

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Validate_ReturnsError_WhenSiteIsNullOrEmpty(string site)
    {
        var request = new MakeBookingRequest(
            site,
            "2077-01-01 09:00",
            5,
            "COVID",            
            GetAttendeeDetails(),
            GetContactDetails()
        );
        
        var result = _sut.Validate(request);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(1);
        result.Errors.Single().PropertyName.Should().Be(nameof(MakeBookingRequest.Site));
    }
    
    [Theory]
    [InlineData("")]
    [InlineData("01-01-2077 09:00")]
    [InlineData("2077-99-31 09:00")]
    [InlineData("2077-01-99 09:00")]
    [InlineData("Not a date 09:00")]
    [InlineData("2077-01-01 :00")]
    [InlineData("2077-01-01 09")]
    [InlineData(null)]
    public void Validate_ReturnsError_WhenFromDateIsInvalid(string from)
    {
        var request = new MakeBookingRequest(
            "1000",
            from,
            5,
            "COVID",
            GetAttendeeDetails(),
            GetContactDetails()
        );
        
        var result = _sut.Validate(request);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(1);
        result.Errors.Single().PropertyName.Should().Be(nameof(MakeBookingRequest.From));
    }
    
    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Validate_ReturnsError_WhenServiceIsNullOrEmpty(string service)
    {
        var request = new MakeBookingRequest(
            "1000",
            "2077-01-01 09:00",
            5,
            service,
            GetAttendeeDetails(),
            GetContactDetails()
        );
        
        var result = _sut.Validate(request);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(1);
        result.Errors.Single().PropertyName.Should().Be(nameof(MakeBookingRequest.Service));
    }
        
    [Fact]
    public void Validate_ReturnsError_WhenAttendeeDetailsIsNull()
    {
        var request = new MakeBookingRequest(
            "1000",
            "2077-01-01 09:00",
            5,
            "COVID",
            null,
            GetContactDetails()
        );
        
        var result = _sut.Validate(request);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(1);
        result.Errors.Single().PropertyName.Should().Be(nameof(MakeBookingRequest.AttendeeDetails));
    }

    [Fact]
    public void Validate_ReturnsError_WhenContactDetailsIsNull()
    {
        var request = new MakeBookingRequest(
            "1000",
            "2077-01-01 09:00",
            5,
            "COVID",
            GetAttendeeDetails(),
            null
        );

        var result = _sut.Validate(request);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(1);
        result.Errors.Single().PropertyName.Should().Be(nameof(MakeBookingRequest.ContactDetails));
    }

    [Fact]
    public void Validate_ContactDetailsCanBeNull_IfProvisional()
    {
        var request = new MakeBookingRequest(
            "1000",
            "2077-01-01 09:00",
            5,
            "COVID",
            GetAttendeeDetails(),
            null,
            true
        );

        var result = _sut.Validate(request);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_ProvisionalBooking_ShouldNotHaveContactDetails()
    {
        var request = new MakeBookingRequest(
            "1000",
            "2077-01-01 09:00",
            5,
            "COVID",
            GetAttendeeDetails(),
            [new ContactItem("email", "test@tempuri.org")],
            true
        );

        var result = _sut.Validate(request);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(1);
        result.Errors.Single().PropertyName.Should().Be(nameof(MakeBookingRequest.ContactDetails));
    }

    [Fact]
    public void Validate_ReturnsError_WhenRequestIsEmpty()
    {
        MakeBookingRequest request = null;
        
        var result = _sut.Validate(request);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(1);
    }

    [Fact]
    public void Validate_ReturnsSuccess_WhenRequestIsValid()
    {
        var request = new MakeBookingRequest(
            "1000",
            "2077-01-01 09:00",
            5,
            "COVID",
            GetAttendeeDetails(),
            GetContactDetails()
        );
        var result = _sut.Validate(request);
        result.IsValid.Should().BeTrue();
        result.Errors.Should().HaveCount(0);            
    }

    private AttendeeDetails GetAttendeeDetails()
    {
        var attendeeDetails = new AttendeeDetails(
            "1234567890",
            "FirstName",
            "LastName",
            "1980-01-01"
        );
        return attendeeDetails;
    }

    private ContactItem[] GetContactDetails()
    {
        return [
            new ContactItem("email", "test@tempuri.org"),
            new ContactItem("phone", "0123456789")
            ];
    }
}
