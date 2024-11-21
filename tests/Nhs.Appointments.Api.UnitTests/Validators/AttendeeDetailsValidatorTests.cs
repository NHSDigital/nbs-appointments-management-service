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
            new DateOnly(1990,01,01));
        
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
            new DateOnly(1990, 01, 01));

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
            new DateOnly(1990, 01, 01));
        
        var result = _sut.Validate(request);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(1);
        result.Errors.Single().PropertyName.Should().Be(nameof(AttendeeDetails.LastName));
    }       
    
    [Fact]
    public void Validate_ReturnsError_WhenDateOfBirthIsInTheFuture()
    {
        var today = DateTime.Now.ToString();
        var request = new AttendeeDetails(
            "1234567890",
            "FirstName",
            "LastName",
            DateOnly.FromDateTime(DateTime.Now.AddDays(5)));
        
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
            new DateOnly(2000, 01,01)
        );
        var result = _sut.Validate(request);
        result.IsValid.Should().BeTrue();
        result.Errors.Should().HaveCount(0);            
    }
}
