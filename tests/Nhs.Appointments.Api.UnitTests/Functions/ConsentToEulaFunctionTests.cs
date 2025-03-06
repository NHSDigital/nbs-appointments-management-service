using System.Text;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using Nhs.Appointments.Api.Functions;
using Nhs.Appointments.Api.Models;
using Nhs.Appointments.Core;

namespace Nhs.Appointments.Api.Tests.Functions;

public class ConsentToEulaFunctionTests
{
    private readonly Mock<IEulaService> _eulaService = new();
    private readonly Mock<ILogger<ConsentToEulaFunction>> _logger = new();
    private readonly Mock<IMetricsRecorder> _metricsRecorder = new();
    private readonly ConsentToEulaFunction _sut;
    private readonly Mock<IUserContextProvider> _userContextProvider = new();
    private readonly Mock<IValidator<ConsentToEulaRequest>> _validator = new();

    public ConsentToEulaFunctionTests()
    {
        _sut = new ConsentToEulaFunction(
            _eulaService.Object,
            _validator.Object,
            _userContextProvider.Object,
            _logger.Object,
            _metricsRecorder.Object);
        _validator.Setup(x => x.ValidateAsync(It.IsAny<ConsentToEulaRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());
    }

    private static HttpRequest CreateRequest(string versionDate = "2020-01-01")
    {
        var context = new DefaultHttpContext();
        var request = context.Request;

        var dto = new { versionDate, };

        var body = JsonConvert.SerializeObject(dto);
        request.Body = new MemoryStream(Encoding.UTF8.GetBytes(body));
        request.Headers.Append("Authorization", "Test 123");
        return request;
    }

    [Fact]
    public async Task RunsAsync_Consents_To_The_Eula()
    {
        _eulaService.Setup(
                x => x.GetEulaVersionAsync())
            .ReturnsAsync(new EulaVersion { VersionDate = new DateOnly(2020, 1, 1) });
        _eulaService.Setup(x => x.ConsentToEula("test@test.com"));

        var testPrincipal = UserDataGenerator.CreateUserPrincipal("test@test.com");
        _userContextProvider.Setup(x => x.UserPrincipal).Returns(testPrincipal);
        var request = CreateRequest();

        var result = await _sut.RunAsync(request, functionContext: null) as ContentResult;

        _eulaService.Verify(x => x.GetEulaVersionAsync(), Times.Once);
        _eulaService.Verify(x => x.ConsentToEula("test@test.com"), Times.Once);
    }

    [Fact]
    public async Task RunsAsync_Does_Not_Update_If_Wrong_Version_Is_Provided()
    {
        _eulaService.Setup(
                x => x.GetEulaVersionAsync())
            .ReturnsAsync(new EulaVersion { VersionDate = new DateOnly(2020, 1, 1) });
        _eulaService.Setup(x => x.ConsentToEula("test@test.com"));

        var testPrincipal = UserDataGenerator.CreateUserPrincipal("test@test.com");
        _userContextProvider.Setup(x => x.UserPrincipal).Returns(testPrincipal);
        var request = CreateRequest("1989-03-01");

        var result = await _sut.RunAsync(request, functionContext: null) as ContentResult;

        result.Should().NotBeNull();
        result.StatusCode.Should().Be(400);
        _eulaService.Verify(x => x.GetEulaVersionAsync(), Times.Once);
        _eulaService.Verify(x => x.ConsentToEula("test@test.com"), Times.Never);
    }
}
