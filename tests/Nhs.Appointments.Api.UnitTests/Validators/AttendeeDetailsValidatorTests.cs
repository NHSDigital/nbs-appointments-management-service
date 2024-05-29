using FluentAssertions;
using Nhs.Appointments.Api.Models;
using Nhs.Appointments.Api.Validators;

namespace Nhs.Appointments.Api.Tests.Validators;

public class AttendeeValidatorTests
{
    private readonly AttendeeDetailsValidator _sut = new();

    [Theory]
    [InlineData("12345678901")]
    [InlineData("123456789")]
    [InlineData("NhsNumbers")]
    [InlineData("")]
    [InlineData(null)]
    public void Validate_ReturnsError_WhenNhsNumberIsInvalid(string nhsNumber)
    {
        var request = new AttendeeDetails(
            nhsNumber, 
            "FirstName", 
            "LastName", 
            "1990-01-01");
        
        var result = _sut.Validate(request);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(1);
        result.Errors.Single().PropertyName.Should().Be(nameof(AttendeeDetails.NhsNumber));
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Validate_ReturnsError_WhenFirstNameIsNullOrEmpty(string firstName)
    {
        var request = new AttendeeDetails(
            "1234567890", 
            firstName, 
            "LastName", 
            "1990-01-01");
        
        var result = _sut.Validate(request);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(1);
        result.Errors.Single().PropertyName.Should().Be(nameof(AttendeeDetails.FirstName));
    }
    
    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Validate_ReturnsError_WhenLastNameIsNullOrEmpty(string lastName)
    {
        var request = new AttendeeDetails(
            "1234567890", 
            "FirstName", 
            lastName, 
            "1990-01-01");
        
        var result = _sut.Validate(request);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(1);
        result.Errors.Single().PropertyName.Should().Be(nameof(AttendeeDetails.LastName));
    }
    
    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("01-01-2077")]
    [InlineData("1990-99-31")]
    [InlineData("1990-01-99")]
    public void Validate_ReturnsError_WhenDateOfBirthIsInvalid(string dateOfBirth)
    {
        var request = new AttendeeDetails(
            "1234567890", 
            "FirstName", 
            "LastName", 
            dateOfBirth);
        
        var result = _sut.Validate(request);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(1);
        result.Errors.Single().PropertyName.Should().Be(nameof(AttendeeDetails.DateOfBirth));
    }
    
    [Fact]
    public void Validate_ReturnsError_WhenDateOfBirthIsInTheFuture()
    {
        var today = DateTime.Now.ToString();
        var request = new AttendeeDetails(
            "1234567890",
            "FirstName",
            "LastName",
            today);
        
        var result = _sut.Validate(request);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(1);
        result.Errors.Single().PropertyName.Should().Be(nameof(AttendeeDetails.DateOfBirth));
    }
    
    [Fact]
    public void Validate_ReturnsSuccess_WhenRequestIsValid()
    {
        var request = new AttendeeDetails(
            "1234567890",
            "FirstName",
            "LastName",
            "2000-01-01"
        );
        var result = _sut.Validate(request);
        result.IsValid.Should().BeTrue();
        result.Errors.Should().HaveCount(0);            
    }
}
