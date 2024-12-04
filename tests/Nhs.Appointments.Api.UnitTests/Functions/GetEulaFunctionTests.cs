using FluentAssertions;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using Nhs.Appointments.Api.Auth;
using Nhs.Appointments.Api.Functions;
using Nhs.Appointments.Api.Models;
using Nhs.Appointments.Core;

namespace Nhs.Appointments.Api.Tests.Functions;

public class GetEulaFunctionTests
{
    private readonly Mock<IEulaService> _eulaService = new();
    private readonly Mock<IValidator<EmptyRequest>> _validator = new();
    private readonly Mock<IUserContextProvider> _userContextProvider = new();
    private readonly Mock<ILogger<GetEulaFunction>> _logger = new();
    private readonly Mock<IMetricsRecorder> _metricsRecorder = new();
    private readonly GetEulaFunction _sut;

    public GetEulaFunctionTests()
    {
        _sut = new GetEulaFunction(
            _eulaService.Object,
            _validator.Object,
            _userContextProvider.Object,
            _logger.Object,
            _metricsRecorder.Object);
    }

    private static HttpRequest CreateRequest()
    {
        var context = new DefaultHttpContext();
        var request = context.Request;
        return request;
    }

    [Fact]
    public async Task RunsAsync_Gets_The_Latest_Eula()
    {
        _eulaService.Setup(
            x => x.GetEulaVersionAsync())
            .ReturnsAsync(new EulaVersion()
            {
                VersionDate = new DateOnly(2020, 1, 1)
            });

        var request = CreateRequest();

        var result = await _sut.RunAsync(request) as ContentResult;

        result?.StatusCode.Should().Be(200);
        var response = await ReadResponseAsync<EulaVersion>(result.Content);

        response.VersionDate.Should().Be(new DateOnly(2020, 1, 1));
        _eulaService.Verify(x => x.GetEulaVersionAsync(), Times.Once);
    }


    private static async Task<TRequest> ReadResponseAsync<TRequest>(string response)
    {
        var body = await new StringReader(response).ReadToEndAsync();
        return JsonConvert.DeserializeObject<TRequest>(body);
    }
}