using FluentAssertions;
using Nhs.Appointments.Api.Models;
using Nhs.Appointments.Api.Validators;

namespace Nhs.Appointments.Api.Tests.Validators;

public class SetSiteDetailsRequestValidatorTests
{
    private readonly SetSiteDetailsRequestValidator _sut = new();

    private readonly SetSiteDetailsRequest _validRequest = new("mock-site-id",
        "Alderney Road Community Pharmacy", "67 Alderney Road, New Pudsey, LS28 7QH", "01234 567890", "-1.234", "53.789"
    );

    [Fact(DisplayName = "Validates a valid request")]
    public void ValidatesTheValidRequest()
    {
        var result = _sut.Validate(_validRequest);
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Theory(DisplayName = "Validates the site id")]
    [InlineData("", "Provide a valid site")]
    [InlineData(null, "Provide a valid site")]
    [InlineData("     ", "Provide a valid site")]
    public void ValidatesTheSiteId(string siteId, string expectedError)
    {
        var request = new SetSiteDetailsRequest(
            siteId,
            _validRequest.Name,
            _validRequest.Address,
            _validRequest.PhoneNumber,
            _validRequest.Longitude,
            _validRequest.Latitude
        );

        var result = _sut.Validate(request);
        result.Errors.Single().ErrorMessage.Should().Be(expectedError);
    }

    [Theory(DisplayName = "Validates the site name")]
    [InlineData("", "Provide a valid name")]
    [InlineData(null, "Provide a valid name")]
    [InlineData("     ", "Provide a valid name")]
    public void ValidatesTheSiteName(string name, string expectedError)
    {
        var request = new SetSiteDetailsRequest(
            _validRequest.Site,
            name,
            _validRequest.Address,
            _validRequest.PhoneNumber,
            _validRequest.Longitude,
            _validRequest.Latitude
        );

        var result = _sut.Validate(request);
        result.Errors.Single().ErrorMessage.Should().Be(expectedError);
    }

    [Theory(DisplayName = "Validates the site address")]
    [InlineData("", "Provide a valid address")]
    [InlineData(null, "Provide a valid address")]
    [InlineData("     ", "Provide a valid address")]
    public void ValidatesTheSiteAddress(string address, string expectedError)
    {
        var request = new SetSiteDetailsRequest(
            _validRequest.Site,
            _validRequest.Name,
            address,
            _validRequest.PhoneNumber,
            _validRequest.Longitude,
            _validRequest.Latitude
        );

        var result = _sut.Validate(request);
        result.Errors.Single().ErrorMessage.Should().Be(expectedError);
    }


    [Theory(DisplayName = "Validates the phone number")]
    [InlineData(" 123a    ", "Phone number must contain numbers and spaces only")]
    [InlineData("123e", "Phone number must contain numbers and spaces only")]
    public void ValidatesThePhoneNumber(string phoneNumber, string expectedError)
    {
        var request = new SetSiteDetailsRequest(
            _validRequest.Site,
            _validRequest.Name,
            _validRequest.Address,
            phoneNumber,
            _validRequest.Longitude,
            _validRequest.Latitude
        );

        var result = _sut.Validate(request);
        result.Errors.Single().ErrorMessage.Should().Be(expectedError);
    }

    [Theory(DisplayName = "Validates the longitude")]
    [InlineData("", "Longitude must be a decimal number")]
    [InlineData(null, "Longitude must be a decimal number")]
    [InlineData("     ", "Longitude must be a decimal number")]
    [InlineData("-8.2", "Longitude must be greater than or equal to -8.1")]
    [InlineData("1.9", "Longitude must be less than or equal to 1.8")]
    public void ValidatesTheLongitude(string longitude, string expectedError)
    {
        var request = new SetSiteDetailsRequest(
            _validRequest.Site,
            _validRequest.Name,
            _validRequest.Address,
            _validRequest.PhoneNumber,
            longitude,
            _validRequest.Latitude
        );

        var result = _sut.Validate(request);
        result.Errors.Single().ErrorMessage.Should().Be(expectedError);
    }

    [Theory(DisplayName = "Validates the latitude")]
    [InlineData("", "Latitude must be a decimal number")]
    [InlineData(null, "Latitude must be a decimal number")]
    [InlineData("     ", "Latitude must be a decimal number")]
    [InlineData("-49.9", "Latitude must be greater than or equal to 49.8")]
    [InlineData("61.0", "Latitude must be less than or equal to 60.9")]
    public void ValidatesTheLatitude(string latitude, string expectedError)
    {
        var request = new SetSiteDetailsRequest(
            _validRequest.Site,
            _validRequest.Name,
            _validRequest.Address,
            _validRequest.PhoneNumber,
            _validRequest.Longitude,
            latitude
        );

        var result = _sut.Validate(request);
        result.Errors.Single().ErrorMessage.Should().Be(expectedError);
    }
}
