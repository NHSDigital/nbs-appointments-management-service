using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using Moq;
using Nhs.Appointments.Api.Functions.HttpFunctions;
using Nhs.Appointments.Api.Models;
using Nhs.Appointments.Core.Features;
using Nhs.Appointments.Core.Reports.Users;
using Nhs.Appointments.Core.Users;
using System.Text;

namespace Nhs.Appointments.Api.Tests.Functions;

public class GetSiteUsersReportFunctionTests
{
    private readonly Mock<IUserService> _userService = new();
    private readonly Mock<IFeatureToggleHelper> _featureToggleHelper = new();
    private readonly Mock<IValidator<EmptyRequest>> _validator = new();
    private readonly Mock<ILogger<GetSiteUsersReportFunction>> _logger = new();
    private readonly Mock<IMetricsRecorder> _metricsRecorder = new();
    private readonly Mock<IUserContextProvider> _userContextProvider = new();
    private readonly Mock<TimeProvider> _timeProvider = new();

    private readonly GetSiteUsersReportFunction _sut;

    public GetSiteUsersReportFunctionTests()
    {
        _timeProvider.Setup(x => x.GetUtcNow())
            .Returns(new DateTimeOffset(new DateTime(2026, 1, 1, 1, 1, 1)));

        _sut = new GetSiteUsersReportFunction(
            _userService.Object,
            new UserCsvWriter(_timeProvider.Object),
            _featureToggleHelper.Object,
            _validator.Object,
            _logger.Object,
            _metricsRecorder.Object,
            _userContextProvider.Object);
        _validator.Setup(v => v.ValidateAsync(It.IsAny<EmptyRequest>()))
            .ReturnsAsync(new ValidationResult());
    }

    [Fact]
    public async Task RunAsync_ReturnsNotImplemented_WhenReportsFeatureFlagIsDisabled()
    {
        _featureToggleHelper.Setup(x => x.IsFeatureEnabled(Flags.ReportsUplift))
            .ReturnsAsync(false);

        var request = CreateRequest();

        var result = await _sut.RunAsync(request) as ContentResult;

        result.StatusCode.Should().Be(501);
    }

    [Fact]
    public async Task RunAsync_GeneratesSiteUsersCsv()
    {
        _featureToggleHelper.Setup(x => x.IsFeatureEnabled(Flags.ReportsUplift))
            .ReturnsAsync(true);
        _userService.Setup(x => x.GetUsersWithPermissionScope(It.IsAny<string>()))
            .ReturnsAsync(new List<User>
            {
                new() { Id = "test.user1@nhs.net" },
                new() { Id = "test.user2@nhs.net" }
            });

        var request = CreateRequest();

        var result = await _sut.RunAsync(request);
        var fileContentResult = Assert.IsType<FileContentResult>(result);

        fileContentResult.ContentType.Should().Be("text/csv");
        fileContentResult.FileDownloadName.Should().Be("UserReport_Sites_20260101010101.csv");
        
        var contentString = Encoding.UTF8.GetString(fileContentResult.FileContents);
        var csvLines = contentString.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
        var headers = csvLines[0].Split(',');

        headers.Length.Should().Be(1);
        headers.First().Should().Be("User");

        csvLines.Length.Should().Be(3);
        csvLines[1].Should().Be("test.user1@nhs.net");
        csvLines[2].Should().Be("test.user2@nhs.net");
    }

    private static HttpRequest CreateRequest()
    {
        var context = new DefaultHttpContext();
        var request = context.Request;
        request.Headers.Append("Authorization", "Test 123");
        context.Request.RouteValues = new RouteValueDictionary
        {
            { "site", "test-site-123" }
        };

        return request;
    }
}
