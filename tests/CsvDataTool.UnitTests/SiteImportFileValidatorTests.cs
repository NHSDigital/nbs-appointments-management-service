using CsvDataTool.Validators;
using FluentAssertions;
using Nhs.Appointments.Core;
using Nhs.Appointments.Persistance.Models;

namespace CsvDataTool.UnitTests;

public class SiteImportFileValidatorTests
{
    private readonly SiteImportFileValidator _sut = new();

    private readonly SiteDocument validSite = new()
    {
        Id = "dc4c51b8-fb7b-4782-a18e-6da741745f36",
        Name = "Test Site",
        OdsCode = "123ABC",
        Address = "13 Old Lane, M25 1PL",
        PhoneNumber = "+44 564 394 273",
        Location = new Location("Point", [1.0, 60.0]),
        IntegratedCareBoard = "789XYZ",
        Region = "Yorkshire",
        Type = "Pharmacy",
        Accessibilities =
        [
            new Accessibility("accessibility/accessible_toilet", "True"),
            new Accessibility("accessibility/braille_translation_service", "true"),
            new Accessibility("accessibility/disabled_car_parking", "false"),
            new Accessibility("accessibility/car_parking", "False"),
            new Accessibility("accessibility/induction_loop", "True"),
            new Accessibility("accessibility/sign_language_service", "False"),
            new Accessibility("accessibility/step_free_access", "True"),
            new Accessibility("accessibility/text_relay", "True"),
            new Accessibility("accessibility/wheelchair_access", "False")
        ]
    };

    [Fact(DisplayName = "Valid list of sites")]
    public void ValidatesTheValidRequest()
    {
        var result = _sut.Validate([validSite]);
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact(DisplayName = "Empty list")]
    public void ValidatesEmptyFiles()
    {
        var result = _sut.Validate(new List<SiteDocument>());
        result.Errors.Single().ErrorMessage.Should().Be("Must upload at least one row");
    }

    [Fact(DisplayName = "Duplicate sites")]
    public void ValidatesDuplicateUsers()
    {
        var result = _sut.Validate([validSite, validSite]);
        result.Errors.Single().ErrorMessage.Should().Be("File contains duplicate rows");
    }

    [Fact(DisplayName = "Includes collection index in error message")]
    public void IncludesIndex()
    {
        var invalidSite = validSite;
        invalidSite.Name = "";

        var result = _sut.Validate([validSite, invalidSite]);

        result.Errors.Should().Contain(error => error.PropertyName == "x[1].Name" &&
                                                error.ErrorMessage == "1: Name must be provided");
    }

    [Theory(DisplayName = "Validates the longitude")]
    [InlineData(-8.2, "0: Longitude must be greater than or equal to -8.1")]
    [InlineData(1.9, "0: Longitude must be less than or equal to 1.8")]
    public void ValidatesTheLongitude(double longitude, string expectedError)
    {
        var invalidSite = validSite;
        invalidSite.Location.Coordinates[0] = longitude;

        var result = _sut.Validate([invalidSite]);
        result.Errors.Single().ErrorMessage.Should().Be(expectedError);
    }

    [Theory(DisplayName = "Validates the latitude")]
    [InlineData(-49.9, "0: Latitude must be greater than or equal to 49.8")]
    [InlineData(61.0, "0: Latitude must be less than or equal to 60.9")]
    public void ValidatesTheLatitude(double latitude, string expectedError)
    {
        var invalidSite = validSite;
        invalidSite.Location.Coordinates[1] = latitude;

        var result = _sut.Validate([invalidSite]);
        result.Errors.Single().ErrorMessage.Should().Be(expectedError);
    }

    [Fact(DisplayName = "Validates the invalid coordinates")]
    public void ValidatesTheLatitude_CouldNotParse()
    {
        var invalidSite = validSite;
        invalidSite.Location = null;

        var result = _sut.Validate([invalidSite]);
        result.Errors.Should().Contain(error => error.ErrorMessage == "0: Longitude must be provided");
        result.Errors.Should().Contain(error => error.ErrorMessage == "0: Latitude must be provided");
    }

    [Theory(DisplayName = "Validates the phone number")]
    [InlineData("", "0: Phone Number must be provided")]
    [InlineData(" 123a    ", "0: Phone Number must be a valid phone number")]
    [InlineData("123e", "0: Phone Number must be a valid phone number")]
    public void ValidatesThePhoneNumber(string phoneNumber, string expectedError)
    {
        var invalidSite = validSite;
        invalidSite.PhoneNumber = phoneNumber;

        var result = _sut.Validate([invalidSite]);
        result.Errors.Should().Contain(error => error.ErrorMessage == expectedError);
    }

    [Theory(DisplayName = "Validates the site ID")]
    [InlineData("", "0: Id must be provided")]
    [InlineData(" dc4c51b8-fb7b-4782-a18e-6da741745f36", "0: Id must not contain whitespace")]
    [InlineData("dc4c51b8-fb7b-4782-a18e-6da741745f36  ", "0: Id must not contain whitespace")]
    [InlineData("dc4c51b8 - fb7b - 4782 -a18e-6da741745f36", "0: Id must not contain whitespace")]
    [InlineData("not-a-guid", "0: Id must be a valid GUID")]
    public void ValidatesTheSiteId(string siteId, string expectedError)
    {
        var invalidSite = validSite;
        invalidSite.Id = siteId;

        var result = _sut.Validate([invalidSite]);
        result.Errors.Should().Contain(error => error.ErrorMessage == expectedError);
    }

    [Theory(DisplayName = "Validates the ODS Code")]
    [InlineData("", "0: Ods Code must be provided")]
    [InlineData("123abc", "0: Ods Code must be uppercase")]
    [InlineData(" 123ABC", "0: Ods Code must not contain whitespace")]
    [InlineData("123ABC ", "0: Ods Code must not contain whitespace")]
    [InlineData("123 ABC", "0: Ods Code must not contain whitespace")]
    [InlineData("123ABC456ZBC", "0: Ods Code must not exceed 10 characters")]
    [InlineData("1B", "0: Ods Code must be at least 3 characters")]
    public void ValidatesTheOdsCode(string odsCode, string expectedError)
    {
        var invalidSite = validSite;
        invalidSite.OdsCode = odsCode;

        var result = _sut.Validate([invalidSite]);
        result.Errors.Should().Contain(error => error.ErrorMessage == expectedError);
    }

    [Theory(DisplayName = "Validates the ICB")]
    [InlineData("", "0: Integrated Care Board must be provided")]
    [InlineData("123abc", "0: Integrated Care Board must be uppercase")]
    [InlineData(" 123ABC", "0: Integrated Care Board must not contain whitespace")]
    [InlineData("123ABC ", "0: Integrated Care Board must not contain whitespace")]
    [InlineData("123 ABC", "0: Integrated Care Board must not contain whitespace")]
    [InlineData("123ABC456ZBC", "0: Integrated Care Board must not exceed 10 characters")]
    [InlineData("1B", "0: Integrated Care Board must be at least 3 characters")]
    public void ValidatesTheIcb(string icb, string expectedError)
    {
        var invalidSite = validSite;
        invalidSite.IntegratedCareBoard = icb;

        var result = _sut.Validate([invalidSite]);
        result.Errors.Should().Contain(error => error.ErrorMessage == expectedError);
    }

    [Theory(DisplayName = "Validates the site name")]
    [InlineData("", "0: Name must be provided")]
    public void ValidatesTheName(string name, string expectedError)
    {
        var invalidSite = validSite;
        invalidSite.Name = name;

        var result = _sut.Validate([invalidSite]);
        result.Errors.Single().ErrorMessage.Should().Be(expectedError);
    }

    [Theory(DisplayName = "Validates the site address")]
    [InlineData("", "0: Address must be provided")]
    public void ValidatesTheAddress(string address, string expectedError)
    {
        var invalidSite = validSite;
        invalidSite.Address = address;

        var result = _sut.Validate([invalidSite]);
        result.Errors.Single().ErrorMessage.Should().Be(expectedError);
    }

    [Theory(DisplayName = "Validates the site region")]
    [InlineData("", "0: Region must be provided")]
    public void ValidatesTheRegion(string region, string expectedError)
    {
        var invalidSite = validSite;
        invalidSite.Region = region;

        var result = _sut.Validate([invalidSite]);
        result.Errors.Single().ErrorMessage.Should().Be(expectedError);
    }

    [Theory(DisplayName = "Validates the site type")]
    [InlineData("", "0: Type must be provided")]
    [InlineData("NotPharmacy", "0: Type must be one of the following: Pharmacy")]
    public void ValidatesTheType(string type, string expectedError)
    {
        var invalidSite = validSite;
        invalidSite.Type = type;

        var result = _sut.Validate([invalidSite]);
        result.Errors.Should().Contain(error => error.ErrorMessage == expectedError);
    }

    [Fact]
    public void ValidatesTheAccessibilityAttributes_OneMissing()
    {
        Accessibility[] accessibilityAttributesMissingBraille =
        [
            new("accessibility/accessible_toilet", "True"),
            new("accessibility/disabled_car_parking", "False"),
            new("accessibility/car_parking", "False"),
            new("accessibility/induction_loop", "True"),
            new("accessibility/sign_language_service", "False"),
            new("accessibility/step_free_access", "True"),
            new("accessibility/text_relay", "True"),
            new("accessibility/wheelchair_access", "False")
        ];

        var invalidSite = validSite;
        invalidSite.Accessibilities = accessibilityAttributesMissingBraille;

        var result = _sut.Validate([invalidSite]);
        result.Errors.Should()
            .Contain(error => error.ErrorMessage == "0: All 9 accessibility attributes must be provided");
    }

    [Fact]
    public void ValidatesTheAccessibilityAttributes_OneMisspelt()
    {
        Accessibility[] accessibilityAttributesWithBrailleMisspelt =
        [
            new("accessibility/accessible_toilet", "True"),
            new("accessibility/disabled_car_parking", "False"),
            new("accessibility/car_parking", "False"),
            new("accessibility/induction_loop", "True"),
            new("accessibility/sign_language_service", "False"),
            new("accessibility/step_free_access", "True"),
            new("accessibility/text_relay", "True"),
            new("accessibility/wheelchair_access", "False")
        ];

        var invalidSite = validSite;
        invalidSite.Accessibilities = accessibilityAttributesWithBrailleMisspelt;

        var result = _sut.Validate([invalidSite]);
        result.Errors.Should()
            .Contain(error => error.ErrorMessage == "0: All 9 accessibility attributes must be provided");
    }

    [Fact]
    public void ValidatesTheAccessibilityAttributes_NotABoolean()
    {
        Accessibility[] accessibilityAttributesWithBrailleMisspelt =
        [
            new("accessibility/accessible_toilet", ""),
            new("accessibility/braille_translation_service", "true"),
            new("accessibility/disabled_car_parking", "false"),
            new("accessibility/car_parking", "False"),
            new("accessibility/induction_loop", "True"),
            new("accessibility/sign_language_service", "False"),
            new("accessibility/step_free_access", "True"),
            new("accessibility/text_relay", "True"),
            new("accessibility/wheelchair_access", "False")
        ];

        var invalidSite = validSite;
        invalidSite.Accessibilities = accessibilityAttributesWithBrailleMisspelt;

        var result = _sut.Validate([invalidSite]);
        result.Errors.Should()
            .Contain(error => error.ErrorMessage == "0: Accessibility attributes must hold the value True or False");
    }
}
