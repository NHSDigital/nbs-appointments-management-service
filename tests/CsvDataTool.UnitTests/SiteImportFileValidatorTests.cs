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
        PhoneNumber = "95827 394 273",
        Location = new Location("Point", [1.0, 60.0]),
        IntegratedCareBoard = "789XYZ",
        Region = "Yorkshire",
        Type = "Pharmacy",
        Accessibilities =
        [
            new Accessibility("accessibility/accessible_toilet", "True"),
            new Accessibility("accessibility/braille_translation_service", "True"),
            new Accessibility("accessibility/disabled_car_parking", "False"),
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
}
