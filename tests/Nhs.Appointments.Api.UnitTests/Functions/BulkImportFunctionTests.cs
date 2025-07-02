using System.Text;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Nhs.Appointments.Api.Factories;
using Nhs.Appointments.Api.Functions;
using Nhs.Appointments.Api.Models;
using Nhs.Appointments.Core;
using Nhs.Appointments.Core.BulkImport;
using Nhs.Appointments.Core.Features;
using Nhs.Appointments.Core.UnitTests;

namespace Nhs.Appointments.Api.Tests.Functions;

public class BulkImportFunctionTests
{
    private readonly Mock<IDataImportHandlerFactory> _mockDataImportFactory = new();
    private readonly Mock<ILogger<BulkImportFunction>> _mockLogger = new();
    private readonly Mock<IMetricsRecorder> _mockMetricsRecorder = new();
    private readonly Mock<ISiteDataImportHandler> _mockSiteDataImporter = new();
    private readonly Mock<IUserContextProvider> _mockUserContextProvider = new();
    private readonly Mock<IUserDataImportHandler> _mockUserDataImporter = new();
    private readonly Mock<IValidator<BulkImportRequest>> _mockValidator = new();
    private readonly Mock<IFeatureToggleHelper> _mockFeatureToggleHelper = new();

    private readonly BulkImportFunction _sut;

    public BulkImportFunctionTests()
    {
        _sut = new BulkImportFunction(
            _mockDataImportFactory.Object,
            _mockValidator.Object,
            _mockUserContextProvider.Object,
            _mockLogger.Object,
            _mockMetricsRecorder.Object);

        _mockValidator.Setup(x => x.ValidateAsync(It.IsAny<BulkImportRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());
    }

    [Fact]
    public async Task RunAsync_ReturnsBadRequest_WhenNoFilesSent()
    {
        var request = CreateBadRequest_NoFiles();

        _mockFeatureToggleHelper.Setup(x => x.IsFeatureEnabled("BulkImport"))
            .ReturnsAsync(true);

        var response = await _sut.RunAsync(request) as ContentResult;

        response.StatusCode.Should().Be(400);
    }

    [Fact]
    public async Task RunAsync_ReturnsBadRequest_WhenMultipleFilesSent()
    {
        var request = CreateBadRequest_MultipleFiles();

        _mockFeatureToggleHelper.Setup(x => x.IsFeatureEnabled("BulkImport"))
            .ReturnsAsync(true);

        var response = await _sut.RunAsync(request) as ContentResult;

        response.StatusCode.Should().Be(400);
    }

    [Fact]
    public async Task RunAsync_UploadsSiteInformation()
    {
        string[] inputRows =
        [
            "ferfgsd,site1,\"test site 1\",\"123 test street\",\"01234 567890\",1.0,60.0,\"test icb1\",\"Yorkshire\",,true,True,False,false,\"true\",false,true,true,false",
            "sadfsdafsdf,site2,\"test site 2\",\"123 test street\",\"01234 567890\",1.0,60.0,\"test icb2\",\"Yorkshire\",,true,True,False,false,\"true\",false,true,true,false"
        ];

        var input = CsvFileBuilder.BuildInputCsv("SiteHeaders", inputRows);

        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(input));
        var file = new FormFile(stream, 0, stream.Length, "Test", "test.csv");

        var request = CreateDefaultRequest();
        request.Form = new FormCollection([], new FormFileCollection { file });

        _mockDataImportFactory.Setup(x => x.CreateDataImportHandler(It.IsAny<string>()))
            .Returns(_mockSiteDataImporter.Object);
        _mockSiteDataImporter.Setup(x => x.ProcessFile(It.IsAny<IFormFile>()))
            .ReturnsAsync(new List<ReportItem> { new(0, "Test 1", true, "") });
        _mockFeatureToggleHelper.Setup(x => x.IsFeatureEnabled("BulkImport"))
            .ReturnsAsync(true);

        var response = await _sut.RunAsync(request) as ContentResult;

        response.StatusCode.Should().Be(200);

        _mockSiteDataImporter.Verify(x => x.ProcessFile(It.IsAny<IFormFile>()), Times.Once);
    }

    [Fact]
    public async Task RunAsync_UploadsUserInformation()
    {
        string[] inputRows =
        [
            "test1@nhs.net,d3793464-b421-41f3-9bfa-53b06e7b3d19",
            "test1@nhs.net,308d515c-2002-450e-b248-4ba36f6667bb",
            "test2@nhs.net,d3793464-b421-41f3-9bfa-53b06e7b3d19",
            "test2@nhs.net,9a06bacd-e916-4c10-8263-21451ca751b8",
        ];

        var input = CsvFileBuilder.BuildInputCsv("SiteHeaders", inputRows);

        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(input));
        var file = new FormFile(stream, 0, stream.Length, "Test", "test.csv");

        var request = CreateDefaultRequest();
        request.Form = new FormCollection([], new FormFileCollection { file });

        _mockDataImportFactory.Setup(x => x.CreateDataImportHandler(It.IsAny<string>()))
            .Returns(_mockUserDataImporter.Object);
        _mockUserDataImporter.Setup(x => x.ProcessFile(It.IsAny<IFormFile>()))
            .ReturnsAsync(new List<ReportItem> { new(0, "Test 1", true, "") });
        _mockFeatureToggleHelper.Setup(x => x.IsFeatureEnabled("BulkImport"))
            .ReturnsAsync(true);

        var response = await _sut.RunAsync(request) as ContentResult;

        response.StatusCode.Should().Be(200);

        _mockUserDataImporter.Verify(x => x.ProcessFile(It.IsAny<IFormFile>()), Times.Once);
    }

    private static HttpRequest CreateBadRequest_MultipleFiles()
    {
        var request = CreateDefaultRequest();
        var file = new FormFile(new MemoryStream(Encoding.UTF8.GetBytes("This is a dummy file")), 0, 0, "Data",
            "dummy.csv");
        var file2 = new FormFile(new MemoryStream(Encoding.UTF8.GetBytes("This is another dummy file")), 0, 0, "Data",
            "dummy2.csv");
        request.Form = new FormCollection([], new FormFileCollection { file, file2 });
        return request;
    }

    private static HttpRequest CreateBadRequest_NoFiles()
    {
        var request = CreateDefaultRequest();
        request.Form = new FormCollection([], new FormFileCollection());
        return request;
    }

    private static HttpRequest CreateDefaultRequest()
    {
        var context = new DefaultHttpContext();
        var request = context.Request;
        request.Headers.Append("Content-Type", "multipart/form-data;");
        request.Headers.Append("Authorization", "Test 123");
        return request;
    }
}
