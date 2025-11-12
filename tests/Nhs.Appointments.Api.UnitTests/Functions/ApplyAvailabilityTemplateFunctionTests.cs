using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Nhs.Appointments.Api.Availability;
using Nhs.Appointments.Api.Functions;
using Nhs.Appointments.Core;
using Nhs.Appointments.Core.Json;
using System.Text;

namespace Nhs.Appointments.Api.Tests.Functions;
public class ApplyAvailabilityTemplateFunctionTests
{
    private readonly Mock<IAvailabilityWriteService> _availabilityWriteService = new();
    private readonly Mock<IValidator<ApplyAvailabilityTemplateRequest>> _validator = new();
    private readonly Mock<IUserContextProvider> _userContextProvider = new();
    private readonly Mock<IMetricsRecorder> _metricsRecorder = new();
    private readonly Mock<ISiteService> _siteService = new();
    private readonly Mock<ILogger<ApplyAvailabilityTemplateFunction>> _logger = new();

    private readonly ApplyAvailabilityTemplateFunction _sut;

    public ApplyAvailabilityTemplateFunctionTests()
    {
        _sut = new ApplyAvailabilityTemplateFunction(
            _availabilityWriteService.Object,
            _validator.Object,
            _userContextProvider.Object,
            _logger.Object,
            _metricsRecorder.Object,
            _siteService.Object);

        _validator.Setup(x => x.ValidateAsync(It.IsAny<ApplyAvailabilityTemplateRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());
    }

    [Fact]
    public async Task RunAsync_ReturnsNotFound_WhenSiteIsNull()
    {
        _siteService.Setup(x => x.GetSiteByIdAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(null as Site);

        var sessions = new List<Session>
        {
            new()
            {
                Capacity = 1,
                From = TimeOnly.FromDateTime(new DateTime(2025, 10, 10, 9, 0, 0)),
                Until = TimeOnly.FromDateTime(new DateTime(2025, 10, 30, 16, 0, 0)),
                Services = ["RSV", "COVID"],
                SlotLength = 5
            }
        }.ToArray();
        var payload = new ApplyAvailabilityTemplateRequest(
            "Test Site 1",
            new DateOnly(2025, 10, 10),
            new DateOnly(2025, 10, 30),
            new Template
            {
                Days = [DayOfWeek.Monday, DayOfWeek.Wednesday, DayOfWeek.Friday],
                Sessions = sessions
            },
            ApplyAvailabilityMode.Additive);

        var request = BuildRequest(payload);
        var result = await _sut.RunAsync(request) as ContentResult;

        result.StatusCode.Should().Be(404);

        _availabilityWriteService.Verify(x => x.ApplyAvailabilityTemplateAsync(
            It.IsAny<string>(),
            It.IsAny<DateOnly>(),
            It.IsAny<DateOnly>(),
            It.IsAny<Template>(),
            It.IsAny<ApplyAvailabilityMode>(),
            It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task RunAsync_AppliesNewAvailabilityTemplate()
    {
        var userPrincipal = UserDataGenerator.CreateUserPrincipal("test.user3@nhs.net");
        _userContextProvider.Setup(x => x.UserPrincipal)
            .Returns(userPrincipal);
        _siteService.Setup(x => x.GetSiteByIdAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(new Site(
                    "test-site",
                    "Test Site",
                    "Test Address",
                    "01234567890",
                    "ODS1",
                    "R1",
                    "ICB1",
                    string.Empty,
                    new List<Accessibility>
                    {
                        new("test_acces/one", "true")
                    },
                    new Location("Coords", [1.234, 5.678]),
                    null,
                    false,
                    string.Empty
                    )); 
        
        var sessions = new List<Session>
        {
            new()
            {
                Capacity = 1,
                From = TimeOnly.FromDateTime(new DateTime(2025, 10, 10, 9, 0, 0)),
                Until = TimeOnly.FromDateTime(new DateTime(2025, 10, 30, 16, 0, 0)),
                Services = ["RSV", "COVID"],
                SlotLength = 5
            }
        }.ToArray();
        var payload = new ApplyAvailabilityTemplateRequest(
            "Test Site 1",
            new DateOnly(2025, 10, 10),
            new DateOnly(2025, 10, 30),
            new Template
            {
                Days = [DayOfWeek.Monday, DayOfWeek.Wednesday, DayOfWeek.Friday],
                Sessions = sessions
            },
            ApplyAvailabilityMode.Additive);

        var request = BuildRequest(payload);
        var result = await _sut.RunAsync(request) as ContentResult;

        _availabilityWriteService.Verify(x => x.ApplyAvailabilityTemplateAsync(
            "Test Site 1",
            new DateOnly(2025, 10, 10),
            new DateOnly(2025, 10, 30),
            It.IsAny<Template>(),
            payload.Mode,
            "test.user3@nhs.net"), Times.Once);
    }

    private static HttpRequest BuildRequest(ApplyAvailabilityTemplateRequest requestBody)
    {
        var body = JsonConvert.SerializeObject(requestBody, new JsonSerializerSettings
        {
            Converters = { new StringEnumConverter(), new ShortTimeOnlyJsonConverter() }
        });
        var context = new DefaultHttpContext();
        var request = context.Request;
        request.Body = new MemoryStream(Encoding.UTF8.GetBytes(body));
        request.Headers.Append("Authorization", "Test 123");

        return request;
    }
}
