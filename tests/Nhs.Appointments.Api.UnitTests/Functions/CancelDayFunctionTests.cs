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
using System.Text;

namespace Nhs.Appointments.Api.Tests.Functions;
public class CancelDayFunctionTests
{

    private readonly Mock<ILogger<CancelDayFunction>> _logger = new();
    private readonly Mock<IMetricsRecorder> _metricsRecorder = new();
    private readonly Mock<IUserContextProvider> _userContextProvider = new();
    private readonly Mock<IValidator<CancelDayRequest>> _validator = new();
    private readonly Mock<IAvailabilityWriteService> _availabilityWriteService = new();

    private readonly CancelDayFunction _sut;

    public CancelDayFunctionTests()
    {
        _sut = new CancelDayFunction(
            _availabilityWriteService.Object,
            _validator.Object,
            _userContextProvider.Object,
            _logger.Object,
            _metricsRecorder.Object);
        _validator.Setup(x => x.ValidateAsync(It.IsAny<CancelDayRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());
    }

    [Fact]
    public async Task RunAsync_ReturnSuccessfulResponse_WhenDayCancelled()
    {
        _availabilityWriteService.Setup(x => x.CancelDayAsync(It.IsAny<string>(), It.IsAny<DateOnly>()))
            .ReturnsAsync((5, 1));

        var request = BuildRequest();

        var response = await _sut.RunAsync(request) as ContentResult;

        response.StatusCode.Should().Be(200);

        var body = await new StringReader(response.Content).ReadToEndAsync();
        var deserialisedResponse = JsonConvert.DeserializeObject<CancelDayResponse>(body);

        deserialisedResponse.BookingsWithoutContactDetails.Should().Be(1);
        deserialisedResponse.CancelledBookingCount.Should().Be(5);

        _availabilityWriteService.Verify(x => x.CancelDayAsync(It.IsAny<string>(), It.IsAny<DateOnly>()), Times.Once);
    }

    private static HttpRequest BuildRequest()
    {
        var requestBody = new
        {
            site = Guid.NewGuid().ToString(),
            date = new DateOnly(2025, 09, 09)
        };
        var body = JsonConvert.SerializeObject(requestBody);
        var context = new DefaultHttpContext();
        var request = context.Request;
        request.Body = new MemoryStream(Encoding.UTF8.GetBytes(body));
        request.Headers.Append("Authorization", "Test 123");

        return request;
    }
}
