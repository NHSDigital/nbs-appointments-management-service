
using FluentAssertions;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using Nhs.Appointments.Api.Functions;
using Nhs.Appointments.Api.Models;
using Nhs.Appointments.Core;
using Nhs.Appointments.Core.Features;

namespace Nhs.Appointments.Api.Tests.Functions;

public class GetClinicalServicesFunctionTests
{
    private readonly GetClinicalServicesFunction _sut;

    private readonly Mock<IValidator<EmptyRequest>> _validator = new();
    private readonly Mock<IUserContextProvider> _userContextProvider = new();
    private readonly Mock<ILogger<GetClinicalServicesFunction>> _logger = new();
    private readonly Mock<IMetricsRecorder> _metricsRecorder = new();
    private readonly Mock<IClinicalServiceStore> _clinicalServiceStore = new();
    private readonly Mock<IFeatureToggleHelper> _featureToggleHelper = new();

    public GetClinicalServicesFunctionTests()
    {
        _sut = new GetClinicalServicesFunction(
            _validator.Object,
            _userContextProvider.Object,
            _logger.Object,
            _metricsRecorder.Object,
            _clinicalServiceStore.Object,
            _featureToggleHelper.Object);
    }

    private static HttpRequest CreateRequest()
    {
        var context = new DefaultHttpContext();
        var request = context.Request;
        return request;
    }

    private static IEnumerable<ClinicalServiceType> MockServices = new List<ClinicalServiceType>() 
    {
        new() 
        { 
            Label = "Service A", 
            Value = "ServiceA" 
        }, 
        new() 
        {
            Label = "Service B",
            Value = "ServiceB" 
        } 
    };

    [Fact]
    public async Task RunsAsync_Returns_501_WhenFeatureDisabled() 
    {
        _featureToggleHelper.Setup(x => x.IsFeatureEnabled(It.Is<string>(x => x.Equals(Flags.MultipleServices)))).ReturnsAsync(false);
        _clinicalServiceStore.Setup(x => x.Get()).ReturnsAsync(MockServices);

        var request = CreateRequest();

        var result = await _sut.RunAsync(request) as ContentResult;
        result?.StatusCode.Should().Be(501);

        _featureToggleHelper.Verify(x => x.IsFeatureEnabled(It.Is<string>(x => x.Equals(Flags.MultipleServices))), Times.Once);
        _clinicalServiceStore.Verify(x => x.Get(), Times.Never);
    }

    [Fact]
    public async Task RunsAsync_Returns_Services_WhenFeatureEnabled()
    {
        _featureToggleHelper.Setup(x => x.IsFeatureEnabled(It.Is<string>(x => x.Equals(Flags.MultipleServices)))).ReturnsAsync(true);
        _clinicalServiceStore.Setup(x => x.Get()).ReturnsAsync(MockServices);

        var request = CreateRequest();

        var result = await _sut.RunAsync(request) as ContentResult;
        var response = await ReadResponseAsync<IEnumerable<ClinicalServiceType>>(result.Content);

        result?.StatusCode.Should().Be(200);
        Assert.Equivalent(MockServices, response);

        _featureToggleHelper.Verify(x => x.IsFeatureEnabled(It.Is<string>(x => x.Equals(Flags.MultipleServices))), Times.Once);
        _clinicalServiceStore.Verify(x => x.Get(), Times.Once);
    }

    private static async Task<TRequest> ReadResponseAsync<TRequest>(string response)
    {
        var body = await new StringReader(response).ReadToEndAsync();
        return JsonConvert.DeserializeObject<TRequest>(body);
    }

}
