using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Nhs.Appointments.Api.Models;
using Nhs.Appointments.Api.Validators;
using Nhs.Appointments.Core.UnitTests;
using System.Text;

namespace Nhs.Appointments.Api.Tests.Validators;

public class BulkImportRequestValidatorTests
{
    private readonly BulkImportRequestValidator _sut = new();

    [Fact]
    public void ShouldFailValidation_WhenFileNotProvided()
    {
        var request = new BulkImportRequest(null, "test");

        var result = _sut.Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Count.Should().Be(1);
    }
    
    [Fact]
    public void ShouldFailValidation_WhenTypeNotProvided()
    {
        var request = new BulkImportRequest(GetFile(), string.Empty);

        var result = _sut.Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Count.Should().Be(1);
    }

    [Fact]
    public void ShouldPassValidation()
    {
        var request = new BulkImportRequest(GetFile(), "test");

        var result = _sut.Validate(request);

        result.IsValid.Should().BeTrue();
    }

    private static FormFile GetFile()
    {
        string[] inputRows =
        [
            "test1@nhs.net,d3793464-b421-41f3-9bfa-53b06e7b3d19",
            "test1@nhs.net,308d515c-2002-450e-b248-4ba36f6667bb",
            "test2@nhs.net,d3793464-b421-41f3-9bfa-53b06e7b3d19",
            "test2@nhs.net,9a06bacd-e916-4c10-8263-21451ca751b8",
        ];

        var input = CsvFileBuilder.BuildInputCsv("User,Site", inputRows);

        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(input));
        return new FormFile(stream, 0, stream.Length, "Test", "test.csv");
    }
}
