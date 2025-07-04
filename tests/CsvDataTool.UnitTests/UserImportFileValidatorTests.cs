using CsvDataTool.Validators;
using FluentAssertions;

namespace CsvDataTool.UnitTests;

public class UserImportFileValidatorTests
{
    private readonly UserImportFileValidator _sut = new();
    private readonly string validSiteId = "874fa3f0-b15b-48f4-8fe5-ef96e85484db";

    private readonly List<UserImportRow> _validFile = new()
    {
        new UserImportRow { UserId = "vincent1@nhs.net", SiteId = "874fa3f0-b15b-48f4-8fe5-ef96e85484db" },
        new UserImportRow { UserId = "kim4@nhs.net", SiteId = "874fa3f0-b15b-48f4-8fe5-ef96e85484db" }
    };

    [Fact(DisplayName = "Valid list of users")]
    public void ValidatesTheValidRequest()
    {
        var result = _sut.Validate(_validFile);
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact(DisplayName = "Empty list")]
    public void ValidatesEmptyFiles()
    {
        var result = _sut.Validate(new List<UserImportRow>());
        result.Errors.Single().ErrorMessage.Should().Be("Must upload at least one row");
    }

    [Fact(DisplayName = "Duplicate users")]
    public void ValidatesDuplicateUsers()
    {
        var result = _sut.Validate(new List<UserImportRow>
        {
            new() { UserId = "vincent1@nhs.net", SiteId = "874fa3f0-b15b-48f4-8fe5-ef96e85484db" },
            new() { UserId = "vincent1@nhs.net", SiteId = "874fa3f0-b15b-48f4-8fe5-ef96e85484db" }
        });
        result.Errors.Single().ErrorMessage.Should().Be("File contains duplicate rows");
    }

    [Fact(DisplayName = "Includes collection index in error message")]
    public void IncludesIndex()
    {
        var result = _sut.Validate([
            new UserImportRow { UserId = "error", SiteId = "error" },
            new UserImportRow { UserId = "error", SiteId = "error" }
        ]);

        result.Errors.Should().Contain(error => error.PropertyName == "x[0].UserId" &&
                                                error.ErrorMessage == "0: User Id must be a valid email address");
        result.Errors.Should().Contain(error => error.PropertyName == "x[0].SiteId" &&
                                                error.ErrorMessage == "0: Site Id must be a valid GUID");
        result.Errors.Should().Contain(error => error.PropertyName == "x[1].UserId" &&
                                                error.ErrorMessage == "1: User Id must be a valid email address");
        result.Errors.Should().Contain(error => error.PropertyName == "x[1].SiteId" &&
                                                error.ErrorMessage == "1: Site Id must be a valid GUID");
    }

    [Theory(DisplayName = "Validates erroneous lines")]
    [InlineData("user-with-space @nhs.net ", "874fa3f0-b15b-48f4-8fe5-ef96e85484db",
        "0: User Id must not contain whitespace")]
    [InlineData(" user-with-leading-space@nhs.net", "874fa3f0-b15b-48f4-8fe5-ef96e85484db",
        "0: User Id must not contain whitespace")]
    [InlineData("user-with-trailing-space@nhs.net ", "874fa3f0-b15b-48f4-8fe5-ef96e85484db",
        "0: User Id must not contain whitespace")]
    [InlineData("", "874fa3f0-b15b-48f4-8fe5-ef96e85484db",
        "0: User Id must be provided", "0: User Id must be a valid email address")]
    [InlineData("UPPERCASE-EMAIL@nhs.net", "874fa3f0-b15b-48f4-8fe5-ef96e85484db",
        "0: User Id must be lowercase")]
    [InlineData("valid-email@nhs.net", " 874fa3f0-b15b-48f4-8fe5-ef96e85484db",
        "0: Site Id must not contain whitespace")]
    [InlineData("valid-email@nhs.net", " 874fa3f0-b15b-48f4-8fe5-ef96e85484db ",
        "0: Site Id must not contain whitespace")]
    [InlineData("valid-email@nhs.net", "874 fa3f0-b1 5b-48 f4-8 fe5-ef96e85484db",
        "0: Site Id must not contain whitespace", "0: Site Id must be a valid GUID")]
    [InlineData("valid-email@nhs.net", " not-a-guid",
        "0: Site Id must be a valid GUID")]
    public void ValidatesTheSiteId(string userId, string siteId, params string[] messages)
    {
        var result = _sut.Validate([new UserImportRow { UserId = userId, SiteId = siteId }]);

        messages.Should().HaveCountGreaterOrEqualTo(1);
        foreach (var message in messages)
        {
            result.Errors.Should().Contain(error => error.ErrorMessage == message);
        }
    }
}
