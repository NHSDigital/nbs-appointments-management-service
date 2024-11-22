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
using FluentValidation.Results;

namespace Nhs.Appointments.Api.Tests.Functions;

public class GetAvailabilityCreatedEventsFunctionTests
{
    private readonly Mock<IAvailabilityService> availabilityService = new();
    private readonly Mock<IValidator<GetAvailabilityCreatedEventsRequest>> _validator = new();
    private readonly Mock<IUserContextProvider> _userContextProvider = new();
    private readonly Mock<ILogger<GetAvailabilityCreatedEventsFunction>> _logger = new();
    private readonly Mock<IMetricsRecorder> _metricsRecorder = new();
    private readonly GetAvailabilityCreatedEventsFunction _sut;

    public GetAvailabilityCreatedEventsFunctionTests()
    {
        _sut = new GetAvailabilityCreatedEventsFunction(
            availabilityService.Object,
            _validator.Object,
            _userContextProvider.Object,
            _logger.Object,
            _metricsRecorder.Object);
        _validator
            .Setup(x => x.ValidateAsync(It.IsAny<GetAvailabilityCreatedEventsRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());
    }

    private static HttpRequest CreateRequest()
    {
        var context = new DefaultHttpContext();
        var request = context.Request;
        request.QueryString = new QueryString($"?site=1000&from=2000-01-01");
        return request;
    }

    [Fact]
    public async Task RunsAsync_Gets_Availability_Created_Events()
    {
        availabilityService.Setup(
            x => x.GetAvailabilityCreatedEventsAsync("1000", DateOnly.FromDateTime(new DateTime(2000, 1, 1))))
            .ReturnsAsync([new AvailabilityCreatedEvent()
            {
                Created = DateTime.UtcNow,
                By = "test@test.com",
                Site = "1000",
                From = DateOnly.FromDateTime(DateTime.Now),
            }]);

        var testPrincipal = UserDataGenerator.CreateUserPrincipal("test@test.com");
        _userContextProvider.Setup(x => x.UserPrincipal).Returns(testPrincipal);
        var request = CreateRequest();

        var result = await _sut.RunAsync(request) as ContentResult;

        result?.StatusCode.Should().Be(200);
        var response = await ReadResponseAsync<IEnumerable<AvailabilityCreatedEvent>>(result.Content);

        response.Single().By.Should().Be("test@test.com");
        availabilityService.Verify(
            x => x.GetAvailabilityCreatedEventsAsync("1000", DateOnly.FromDateTime(new DateTime(2000, 1, 1))),
            Times.Once);
    }


    private static async Task<TRequest> ReadResponseAsync<TRequest>(string response)
    {
        var body = await new StringReader(response).ReadToEndAsync();
        return JsonConvert.DeserializeObject<TRequest>(body);
    }
}