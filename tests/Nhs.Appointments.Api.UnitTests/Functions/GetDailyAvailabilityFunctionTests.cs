using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using Nhs.Appointments.Api.Functions;
using Nhs.Appointments.Api.Json;
using Nhs.Appointments.Api.Models;
using Nhs.Appointments.Core;

namespace Nhs.Appointments.Api.Tests.Functions
{
    public class GetDailyAvailabilityFunctionTests
    {
        private readonly Mock<IAvailabilityQueryService> _mockAvailabilityQueryService = new();
        private readonly Mock<ILogger<GetDailyAvailabilityFunction>> _mockLogger = new();
        private readonly Mock<IMetricsRecorder> _mockMetricsRecorder = new();
        private readonly Mock<IUserContextProvider> _mockUserContextProvider = new();
        private readonly Mock<IValidator<GetDailyAvailabilityRequest>> _mockValidator = new();

        private readonly GetDailyAvailabilityFunction _sut;

        public GetDailyAvailabilityFunctionTests()
        {
            _sut = new GetDailyAvailabilityFunction(
                _mockAvailabilityQueryService.Object,
                _mockValidator.Object,
                _mockUserContextProvider.Object,
                _mockLogger.Object,
                _mockMetricsRecorder.Object);

            _mockValidator.Setup(x =>
                    x.ValidateAsync(It.IsAny<GetDailyAvailabilityRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());
        }

        [Fact]
        public async Task RunAsync_ReturnsDailyAvailability()
        {
            var fromDate = DateOnly.FromDateTime(new DateTime(2024, 12, 1));
            var toDate = DateOnly.FromDateTime(new DateTime(2024, 12, 8));

            var availability = new List<DailyAvailability>
            {
                new()
                {
                    Date = DateOnly.FromDateTime(new DateTime(2024, 12, 1)),
                    Sessions =
                    [
                        new()
                        {
                            From = new TimeOnly(11, 00),
                            Until = new TimeOnly(16, 00),
                            Capacity = 2,
                            SlotLength = 5,
                            Services = ["RSV:Adult"]
                        }
                    ]
                },
                new()
                {
                    Date = DateOnly.FromDateTime(new DateTime(2024, 12, 4)),
                    Sessions =
                    [
                        new()
                        {
                            From = new TimeOnly(11, 00),
                            Until = new TimeOnly(16, 00),
                            Capacity = 2,
                            SlotLength = 5,
                            Services = ["RSV:Adult"]
                        }
                    ]
                }
            };

            _mockAvailabilityQueryService.Setup(x =>
                    x.GetDailyAvailability(It.IsAny<string>(), It.IsAny<DateOnly>(), It.IsAny<DateOnly>()))
                .ReturnsAsync(availability);

            var request = CreateRequest();

            var result = await _sut.RunAsync(request) as ContentResult;

            result.StatusCode.Should().Be(200);

            var dailyAvailabilities =
                (await ReadResponseAsync<IEnumerable<DailyAvailability>>(result.Content)).ToList();

            dailyAvailabilities.Should().NotBeNull();
            dailyAvailabilities.Any().Should().BeTrue();
            dailyAvailabilities.Count().Should().Be(2);
        }

        private static HttpRequest CreateRequest()
        {
            var context = new DefaultHttpContext();
            var request = context.Request;
            request.QueryString = new QueryString("?site=TEST01&from=2024-12-01&until=2024-12-08");
            request.Headers.Append("Authorization", "Test 123");
            return request;
        }

        private static async Task<TRequest> ReadResponseAsync<TRequest>(string response)
        {
            var deserializerSettings = new JsonSerializerSettings
            {
                Converters = { new ShortTimeOnlyJsonConverter() },
            };
            var body = await new StringReader(response).ReadToEndAsync();
            return JsonConvert.DeserializeObject<TRequest>(body, deserializerSettings);
        }
    }
}
